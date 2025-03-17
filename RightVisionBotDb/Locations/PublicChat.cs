using DryIoc.ImTools;
using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Data.Contexts;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Text;
using RightVisionBotDb.Text.Interfaces;
using RightVisionBotDb.Types;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Locations
{
    public sealed class PublicChat : RvLocation
    {

        #region Properties

        private Dictionary<Type, Func<Lang, IFormMessages>> FormHandlerMessages { get; }


        #endregion

        #region Constructor

        public PublicChat(
            Bot bot,
            LocationService locationService,
            RvLogger logger,
            LocationsFront locationsFront)
            : base(bot, locationService, logger, locationsFront)
        {
            FormHandlerMessages = new()
            {
                { typeof(ParticipantForm), lang => Phrases.Lang[lang].Messages.Participant },
                { typeof(CriticForm), lang => Phrases.Lang[lang].Messages.Critic },
                { typeof(StudentForm), lang => Phrases.Lang[lang].Messages.Academy }
            };

            this
                .RegisterTextCommand("/profile", ProfileCommand)
                .RegisterTextCommand("/ban", BanCommand, Permission.Ban)
                .RegisterTextCommand("/unban", UnbanCommand, Permission.Unban)
                .RegisterTextCommand("/mute", MuteCommand, Permission.Mute)
                .RegisterTextCommand("/unmute", UnmuteCommand, Permission.Unmute)
                .RegisterTextCommand("+reward", AddReward, Permission.Rewarding)
                .RegisterTextCommand("-reward", RemoveReward, Permission.Rewarding)
                .RegisterTextCommand("-судейство", CancelCritic, Permission.Curate)
                .RegisterTextCommands(["+permission", "-permission", "~permission"], AddOrRemovePermissionCommand, Permission.GivePermission)
                .RegisterCallbackCommands(["c_take", "p_take", "st_take"], HandleCuratorshipAsync, Permission.Curate)
                .RegisterCallbackCommand("c_form", CriticFormCallback)
                .RegisterCallbackCommand("p_form", ParticipantFormCallback)
                .RegisterCallbackCommand("st_form", StudentFormCallback);
        }

        #endregion

        #region Methods

        private async Task ProfileCommand(CommandContext c, CancellationToken token)
        {
            var args = c.Message.Text!.Split(' ');

            var (extractedRvUser, _) = args.Length > 1 || c.Message.ReplyToMessage != null
                ? await CommandFormatHelper.ExtractRvUserFromArgs(c, token)
                : (c.RvUser, null);

            if (extractedRvUser == null)
            {
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, Phrases.Lang[c.RvUser.Lang].Messages.Common.UserNotFound, cancellationToken: token);
                return;
            }

            (string message, InlineKeyboardMarkup? keyboard) = await ProfileHelper.Profile(extractedRvUser, c, c.Message.Chat.Type, App.Configuration.RightVisionSettings.DefaultRightVision, token: token);

            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        private async Task BanCommand(CommandContext c, CancellationToken token)
        {
            var (targetRvUser, message, reason, endDate) = await HandeRestrictionAsync(c, PunishmentType.Ban, token);

            await Bot.Client.SendTextMessageAsync(c.Message.Chat, message, cancellationToken: token);

            if (targetRvUser != null)
            {
                await Bot.Client.BanChatMemberAsync(c.Message.Chat, targetRvUser.UserId, endDate, cancellationToken: token);

                targetRvUser.Punishments.Add(new(PunishmentType.Ban, c.Message.Chat.Id, c.RvUser.UserId, reason, DateTime.Now, endDate));
                ((ApplicationDbContext)c.DbContext).Entry(targetRvUser).State = EntityState.Modified;
            }
        }

        private async Task UnbanCommand(CommandContext c, CancellationToken token = default)
        {
            var (extractedRvUser, message) = await HandleUnrestrictionAsync(c, PunishmentType.Ban, token);

            if (extractedRvUser != null)
            {
                await Bot.Client.UnbanChatMemberAsync(c.Message.Chat, extractedRvUser.UserId, true, token);
            }
            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                message,
                cancellationToken: token);
        }

        private async Task MuteCommand(CommandContext c, CancellationToken token = default)
        {
            var (targetRvUser, message, reason, endDate) = await HandeRestrictionAsync(c, PunishmentType.Mute, token);

            await Bot.Client.SendTextMessageAsync(c.Message.Chat, message, cancellationToken: token);

            if (targetRvUser != null)
            {
                await Bot.Client.RestrictChatMemberAsync(
                    c.Message.Chat,
                    targetRvUser.UserId,
                    new ChatPermissions
                    {
                        CanSendMessages = false,
                        CanSendAudios = false,
                        CanSendPolls = false,
                        CanSendOtherMessages = false,
                        CanInviteUsers = false,
                        CanPinMessages = false,
                        CanSendDocuments = false,
                        CanSendPhotos = false,
                        CanSendVideos = false,
                        CanSendVoiceNotes = false,
                        CanSendVideoNotes = false
                    },
                untilDate: endDate,
                cancellationToken: token);

                targetRvUser.Punishments.Add(new(PunishmentType.Mute, c.Message.Chat.Id, c.RvUser.UserId, reason, DateTime.Now, endDate));
                ((ApplicationDbContext)c.DbContext).Entry(targetRvUser).State = EntityState.Modified;
            }
        }

        private async Task UnmuteCommand(CommandContext c, CancellationToken token = default)
        {
            var (extractedRvUser, message) = await HandleUnrestrictionAsync(c, PunishmentType.Mute, token);

            if (extractedRvUser != null)
            {
                await Bot.Client.RestrictChatMemberAsync(
                    c.Message.Chat,
                    extractedRvUser.UserId,
                    new ChatPermissions
                    {
                        CanAddWebPagePreviews = true,
                        CanSendAudios = true,
                        CanSendDocuments = true,
                        CanSendMessages = true,
                        CanSendPhotos = true,
                        CanSendPolls = true,
                        CanSendVideos = true,
                        CanSendVoiceNotes = true,
                        CanSendVideoNotes = true,
                        CanSendOtherMessages = true
                    },
                    cancellationToken: token);
            }
            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                message,
                cancellationToken: token);
        }

        private async Task AddOrRemovePermissionCommand(CommandContext c, CancellationToken token = default)
        {
            var (extractedRvUser, args) = await CommandFormatHelper.ExtractRvUserFromArgs(c, token);

            bool dataUpdated = false;

            string resultMessage = "Пользователь не найден или не указан!";

            if (extractedRvUser != null)
            {
                if (extractedRvUser == c.RvUser)
                    resultMessage = "Извини, но ты не можешь изменять права у самого себя!";

                else if (c.Message.Text!.StartsWith('~'))
                {
                    extractedRvUser.ResetPermissions();
                    resultMessage = $"Выполнен сброс прав до стандартных для пользователя.\n\nИспользованные шаблоны:\n{extractedRvUser.Status}\n{extractedRvUser.Role}";
                    dataUpdated = true;
                }

                else if (Enum.TryParse(args.Last(), out Permission permission))
                {
                    var actionType = c.Message.Text!.First();
                    switch (actionType)
                    {
                        case '+':
                            extractedRvUser.UserPermissions += permission;
                            resultMessage = $"Пользователю успешно выдано право Permission.{permission}";
                            dataUpdated = true;
                            break;
                        case '-':
                            extractedRvUser.UserPermissions -= permission;
                            resultMessage = $"С пользователя успешно снято право Permission.{permission}";
                            dataUpdated = true;
                            break;
                    }
                }
                else
                    resultMessage = "Запрашиваемое право не найдено!";

                if (dataUpdated)
                    ((ApplicationDbContext)c.DbContext).Entry(extractedRvUser).State = EntityState.Modified;
            }

            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                resultMessage,
                cancellationToken: token);
        }

        private async Task AddReward(CommandContext c, CancellationToken token = default)
        {
            var (extractedRvUser, args) = await CommandFormatHelper.ExtractRvUserFromArgs(c, token); //[0]Icon, [1 ..]Description

            var resultMessage = "Пользователь не найден или не указан!";

            if (extractedRvUser != null)
            {
                if (extractedRvUser == c.RvUser)
                    resultMessage = "Извини, но ты не можешь выдавать награду самому себе!";

                else
                {
                    try
                    {
                        var description = string.Join(' ', args.Skip(1));
                        extractedRvUser.Rewards.Add(new Reward(args[0], description));
                        ((ApplicationDbContext)c.DbContext).Entry(extractedRvUser).State = EntityState.Modified;
                        resultMessage = "Пользователю успешно выдана награда!";
                    }
                    catch
                    {
                        resultMessage = "Не удалось выдать награду! Возможно, в команде допущена синтаксическая ошибка.";
                    }
                }
            }

            await Bot.Client.SendTextMessageAsync(c.Message.Chat, resultMessage, cancellationToken: token);
        }

        private async Task RemoveReward(CommandContext c, CancellationToken token = default)
        {
            (var extractedRvUser, var args) = await CommandFormatHelper.ExtractRvUserFromArgs(c, token);
            string message;

            if (extractedRvUser == null)
                message = "Пользователь не указан или не найден!";

            else if (!int.TryParse(args.First(), out var rewardIndex) || extractedRvUser.Rewards.Count < rewardIndex - 1)
                message = "Индекс награды указан неверно!";

            else
            {
                extractedRvUser.Rewards.RemoveAt(rewardIndex - 1);
                ((ApplicationDbContext)c.DbContext).Entry(extractedRvUser).State = EntityState.Modified;
                message = "Награда успешно снята!";
            }

            await Bot.Client.SendTextMessageAsync(c.Message.Chat, message, cancellationToken: token);
        }

        private async Task CancelCritic(CommandContext c, CancellationToken token)
        {
            (var extractedRvUser, var args) = await CommandFormatHelper.ExtractRvUserFromArgs(c, token);
            string message;

            if (extractedRvUser == null)
                message = "Пользователь не указан или не найден!";

            else if (extractedRvUser.Role > c.RvUser.Role)
                message = "Извини, но ты не можешь снимать судейство с пользователя, чья должность выше твоей!";

            else
            {
                var form = await c.DbContext.CriticForms.FirstOrDefaultAsync(critic => extractedRvUser.Is(critic), token);

                if (form != null)
                {
                    c.DbContext.CriticForms.Remove(form);
                    message = "С пользователя успешно снято судейство!";

                    await Bot.Client.SendTextMessageAsync(
                        extractedRvUser.UserId, 
                        Phrases.Lang[extractedRvUser.Lang].Messages.Critic.FormCanceled, 
                        cancellationToken: token);
                }
                else
                    message = "Пользователь итак не является судьёй!";
            }

            await Bot.Client.SendTextMessageAsync(c.Message.Chat, message, cancellationToken: token);
        }

        private async Task CriticFormCallback(CallbackContext c, CancellationToken token = default)
        {
            var args = c.CallbackQuery.Data!.Split('-');
            var userId = long.Parse(args.Last());

            var targetRvUser = c.DbContext.RvUsers.First(u => u.UserId == userId);
            var form = c.DbContext.CriticForms.First(c => c.UserId == userId);

            if (form != null)
            {
                form.Status = await HandleFormAsync(c, targetRvUser, form, args, token);
                switch (form.Status)
                {
                    case FormStatus.Accepted:
                        form.Category = Enum.Parse<Category>(args[1]);
                        targetRvUser.UserPermissions -= Permission.SendCriticForm;
                        break;
                    case FormStatus.Reset:
                        c.DbContext.CriticForms.Remove(form);
                        break;
                }
            }
            else
                await Bot.Client.EditMessageTextAsync(
                    c.CallbackQuery.Message!.Chat,
                    c.CallbackQuery.Message.MessageId,
                    "Ошибка: заявки не существует.",
                    cancellationToken: token);
        }

        private async Task ParticipantFormCallback(CallbackContext c, CancellationToken token = default)
        {
            var args = c.CallbackQuery.Data!.Split('-');
            var userId = long.Parse(args.Last());

            var targetRvUser = c.DbContext.RvUsers.First(u => u.UserId == userId);
            var form = c.RvContext.ParticipantForms.First(c => c.UserId == userId);

            if (form != null)
            {
                form.Status = await HandleFormAsync(c, targetRvUser, form, args, token);
                switch (form.Status)
                {
                    case FormStatus.Accepted:
                        form.Category = Enum.Parse<Category>(args[1]);
                        targetRvUser.UserPermissions -= Permission.SendCriticForm;
                        break;
                    case FormStatus.Reset:
                        c.RvContext.ParticipantForms.Remove(form);
                        break;
                }
            }
            else
                await Bot.Client.EditMessageTextAsync(
                    c.CallbackQuery.Message!.Chat,
                    c.CallbackQuery.Message.MessageId,
                    "Ошибка: заявки не существует.",
                    cancellationToken: token);
        }

        private async Task StudentFormCallback(CallbackContext c, CancellationToken token = default)
        {
            var args = c.CallbackQuery.Data!.Split('-');
            var userId = long.Parse(args.Last());

            var targetRvUser = await c.DbContext.RvUsers.FirstAsync(u => u.UserId == userId, token);
            var form = await c.AcademyContext.StudentForms.FirstAsync(c => c.UserId == userId, token);

            if (form != null)
            {
                form.Status = await HandleFormAsync(c, targetRvUser, form, args, token);
                switch (form.Status)
                {
                    case FormStatus.Accepted:
                        targetRvUser.UserPermissions -= Permission.SendCriticForm;
                        break;
                    case FormStatus.Reset:
                        c.AcademyContext.StudentForms.Remove(form);
                        break;
                }
            }
            else
                await Bot.Client.EditMessageTextAsync(
                    c.CallbackQuery.Message!.Chat,
                    c.CallbackQuery.Message.MessageId,
                    "Ошибка: заявки не существует.",
                    cancellationToken: token);
        }

        #region Handle methods

        private async Task HandleCuratorshipAsync(CallbackContext c, CancellationToken token = default)
        {
            var form = await ExtractFormFromArgs(c, c.AcademyContext.StudentForms, token);
            var callback = c.CallbackQuery;

            (string message, InlineKeyboardMarkup? keyboard) =
                form == null
                ? ("Ошибка: заявки не существует.", null)
                : ($"{callback.Message!.Text}\n\n[{DateTime.Now.ToString("g", new CultureInfo("ru-RU"))}] 🤵‍♂️Назначен куратор: {callback.From.FirstName}", KeyboardsHelper.CandidateOptions(form));

            if (form != null)
            {
                form.CuratorId = callback.From.Id;
                form.Status = FormStatus.UnderConsideration;
            }

            await Bot.Client.EditMessageTextAsync(
                callback.Message!.Chat,
                callback.Message.MessageId,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        private async Task<FormStatus> HandleFormAsync(CallbackContext c, RvUser targetRvUser, IForm form, string[] args, CancellationToken token = default)
        {
            if (c.RvUser.UserId != form.CuratorId) return FormStatus.Waiting;

            var callbackType = args[1];
            var formattedDate = DateTime.Now.ToString("g", new CultureInfo("ru-RU"));
            var formHandlerMessages = FormHandlerMessages[form.GetType()](targetRvUser.Lang);
            var messageText = c.CallbackQuery.Message!.Text;

            (string messageForSender, string formText, FormStatus formStatus, InlineKeyboardMarkup? keyboard) = callbackType switch
            {
                "deny" =>
                (
                    string.Format(formHandlerMessages.FormDenied, form.Name, $"{c.CallbackQuery.From.Username}"),
                    $"{messageText}\n[{formattedDate}] ❌Заявка отклонена!",
                    FormStatus.Denied,
                    null
                ),
                "requestPM" =>
                (
                    string.Format(formHandlerMessages.PMRequested, form.Name, $"@{c.CallbackQuery.From.Username}"),
                    $"{messageText}\n[{formattedDate}] 📩Запрошено личное сообщение",
                    FormStatus.Waiting,
                    KeyboardsHelper.CandidateOptions(form)
                ),
                "reset" =>
                (
                    string.Format(formHandlerMessages.FormCanceled, form.Name, $"{c.CallbackQuery.From.FirstName}"),
                    $"{messageText}\n[{formattedDate}] ⚠️Заявка сброшена!",
                    FormStatus.Reset,
                    null
                ),
                "accept" => //Academy only
                (
                    string.Format(formHandlerMessages.FormAccepted, form.Name, Constants.Links.AcademyGeneralGroup, c.CallbackQuery.From.FirstName),
                    $"{messageText}\n[{formattedDate}] ✅Заявка принята!",
                    FormStatus.Accepted,
                    null
                ),
                "Bronze"
                or "Silver"
                or "Gold"
                or "Brilliant" =>
                (
                    string.Format(formHandlerMessages.FormAccepted, form.Name, Phrases.GetCategoryString(Enum.Parse<Category>(callbackType)), $"{c.CallbackQuery.From.FirstName}"),
                    $"{messageText}\n[{formattedDate}] ✅Заявка принята! Категория: {Enum.Parse<Category>(callbackType)}",
                    FormStatus.Accepted,
                    null
                ),
                _ => throw new InvalidDataException(nameof(c.CallbackQuery.Data))
            };
            try
            {
                await Bot.Client.SendTextMessageAsync(
                    form.UserId,
                    messageForSender,
                    replyMarkup: KeyboardsHelper.ReplyMainMenu,
                    cancellationToken: token);

                form.Status = formStatus;

                await Bot.Client.EditMessageTextAsync(
                    c.CallbackQuery.Message!.Chat,
                    c.CallbackQuery.Message.MessageId,
                    formText,
                    replyMarkup: keyboard,
                    cancellationToken: token);
            }
            catch (Exception ex)
            {
                await Bot.Client.SendTextMessageAsync(
                    c.CallbackQuery.Message!.Chat,
                    $"Во время обработки заявки пользователя @{targetRvUser.Telegram} ({targetRvUser.UserId}) произошла ошибка!\n{ex.Message}",
                    cancellationToken: token);
            }
            return formStatus;
        }

        private async Task<(RvUser? targetRvUser, string message, string reason, DateTime endDate)> HandeRestrictionAsync(CommandContext c, PunishmentType punishmentType, CancellationToken token = default)
        {
            var (extractedRvUser, args) = await CommandFormatHelper.ExtractRvUserFromArgs(c, token);

            string message;
            if (extractedRvUser == null || extractedRvUser == c.RvUser || c.RvUser.Role <= extractedRvUser.Role)
            {
                message = extractedRvUser == null
                    ? "Пользователь не указан или не найден!"
                    : extractedRvUser == c.RvUser
                        ? "Извини, но ты не можешь выдать наказание самому себе!"
                        : "Извини, но ты не можешь выдать наказание пользователю, должность которого выше твоей!";

                return (null, message, string.Empty, DateTime.MinValue);
            }
            int minutes = 60;

            if (args.Length > 0)
            {
                var arg = args.First();
                if (arg.Length > 1 && char.IsLetter(arg[^1]) && int.TryParse(arg[..^1], out var timeValue))
                {
                    minutes = arg[^1] switch
                    {
                        'm' => timeValue,
                        'h' => timeValue * 60,
                        _ => 60
                    };
                    args = args.RemoveAt(0);
                }
                else if (int.TryParse(arg, out var plainMinutes))
                {
                    minutes = plainMinutes;
                    args = args.RemoveAt(0);
                }
            }

            if (minutes < 0)
                return (null, "Временной промеуток не может быть меньше нуля!", string.Empty, DateTime.MinValue);


            string reason = args.Length > 0 ? string.Join(" ", args) : Phrases.Lang[extractedRvUser.Lang].Profile.Punishments.Punishment.NoReason ?? "Не указано";

            var endDate = DateTime.Now.AddMinutes(minutes);

            (string restrictionTypeString, string translatedRestrictionType, string whoGaveRestrictionString) = punishmentType switch
            {
                PunishmentType.Ban => ("бан", Phrases.Lang[extractedRvUser.Lang].Profile.Punishments.Punishment.Ban, "Забанил"),
                PunishmentType.Mute => ("мут", Phrases.Lang[extractedRvUser.Lang].Profile.Punishments.Punishment.Mute, "Замутил"),
                _ => (string.Empty, string.Empty, string.Empty)
            };

            message = $"Пользователь {extractedRvUser.Name} получает {restrictionTypeString} в группе!\n\n- {whoGaveRestrictionString}: {c.RvUser.Name} (@{c.RvUser.Telegram})\n- По причине: {reason}\n- До: {endDate.ToString("g", new CultureInfo("ru-RU"))}";

            await Bot.Client.SendTextMessageAsync(
                extractedRvUser.UserId,
                string.Format(
                    Phrases.Lang[extractedRvUser.Lang].Profile.Punishments.Notification,
                    extractedRvUser.Name,
                    translatedRestrictionType,
                    Phrases.GetGroupTypeString(c.Message.Chat.Id, extractedRvUser.Lang),
                    endDate.ToString("g", new CultureInfo("ru-RU")),
                    reason) 
                + Phrases.Lang[extractedRvUser.Lang].Profile.Punishments.Contacts,
                cancellationToken: token);

            return (extractedRvUser, message, reason, endDate);
        }

        private async Task<(RvUser? targetRvUser, string message)> HandleUnrestrictionAsync(CommandContext c, PunishmentType punishmentType, CancellationToken token = default)
        {
            var (extractedRvUser, args) = await CommandFormatHelper.ExtractRvUserFromArgs(c, token);

            string message;
            if (extractedRvUser == null || extractedRvUser == c.RvUser || c.RvUser.Role <= extractedRvUser.Role)
            {
                message = extractedRvUser == null
                    ? "Пользователь не найден или не указан!"
                    : extractedRvUser == c.RvUser
                        ? "Извини, но ты не можешь снять наказание с самого себя!"
                        : "Извини, но ты не можешь снять наказание с пользователя, должность которого выше твоей!";

                return (null, message);
            }

            var restrictionTypeString = punishmentType switch
            {
                PunishmentType.Ban => "бан",
                PunishmentType.Mute => "мут",
                _ => string.Empty
            };

            message = $"Пользователю {extractedRvUser.Name} снимается {restrictionTypeString} в группе!";

            var lang = extractedRvUser.Lang;
            var groupId = c.Message.Chat.Id;
            (string rvUserName, string groupType, string groupLink) = (extractedRvUser.Name, Phrases.GetGroupTypeString(groupId, lang).TrimEnd(), Phrases.GetGroupLink(groupId));

            var notification = punishmentType switch
            {
                PunishmentType.Ban => string.Format(Phrases.Lang[lang].Profile.Punishments.UnbanNotification, rvUserName, groupType, groupLink),
                PunishmentType.Mute => string.Format(Phrases.Lang[lang].Profile.Punishments.UnmuteNotification, rvUserName, groupType, groupLink),
                _ => string.Empty
            };

            var activePunishments = extractedRvUser.Punishments
                .Where(p => p.Type == punishmentType && p.EndDateTime > DateTime.Now)
                .ToList();

            if (activePunishments.Count != 0)
            {
                foreach (var punishment in activePunishments)
                    punishment.EndDateTime = DateTime.Now;

                ((ApplicationDbContext)c.DbContext).Entry(extractedRvUser).State = EntityState.Modified;

                await Bot.Client.SendTextMessageAsync(
                    extractedRvUser.UserId,
                    notification,
                    cancellationToken: token);
            }
            else
            {
                message = $"В настоящий момент {extractedRvUser.Name} не имеет активных наказаний заданного типа!"; 
                return (null, message);
            }

            return (extractedRvUser, message);
        }

        private async Task<IForm> ExtractFormFromArgs<TForm>(CallbackContext callbackContext, IQueryable<TForm> forms, CancellationToken token = default) 
            where TForm : IForm
        {
            var args = callbackContext.CallbackQuery.Data!.Split('-');
            var userId = long.Parse(args.Last());

            return await forms.FirstAsync(f => f.UserId == userId, token);
        }

        #endregion

        #endregion

    }
}
