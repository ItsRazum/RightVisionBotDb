﻿using DryIoc.ImTools;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Lang.Interfaces;
using RightVisionBotDb.Models;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Locations
{
    internal sealed class PublicChat : RvLocation
    {

        #region Properties

        private Dictionary<Type, Func<Enums.Lang, IFormMessages>> FormHandlerMessages { get; }

        #endregion

        #region Constructor

        public PublicChat(
            Bot bot,
            LocationManager locationManager,
            RvLogger logger,
            LocationsFront locationsFront)
            : base(bot, locationManager, logger, locationsFront)
        {
            FormHandlerMessages = new()
            {
                { typeof(ParticipantForm), lang => Language.Phrases[lang].Messages.Participant },
                { typeof(CriticForm), lang => Language.Phrases[lang].Messages.Critic }
            };

            this
                .RegisterTextCommand("/profile", ProfileCommand)
                .RegisterTextCommand("/ban", BanCommand, Permission.Ban)
                .RegisterTextCommand("/mute", MuteCommand, Permission.Mute)
                .RegisterCallbackCommand("permissions_minimized", PermissionsCallback)
                .RegisterCallbackCommand("permissions_maximized", PermissionsCallback)
                .RegisterCallbackCommand("permissions_back", PermissionsBackCallback)
                .RegisterCallbackCommand("c_take", CriticTakeCallback, Permission.Curate)
                .RegisterCallbackCommand("p_take", ParticipantTakeCallback, Permission.Curate)
                .RegisterCallbackCommand("c_form", CriticFormCallback)
                .RegisterCallbackCommand("p_form", ParticipantFormCallback);
        }

        #endregion

        #region Methods

        private async Task ProfileCommand(CommandContext c, CancellationToken token)
        {
            var (extractedRvUser, _) = CommandFormatHelper.ExtractRvUserFromArgs(c);

            if (extractedRvUser == null)
            {
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, Language.Phrases[c.RvUser.Lang].Messages.Common.UserNotFound, cancellationToken: token);
                return;
            }
            (string message, InlineKeyboardMarkup? keyboard) = await ProfileHelper.Profile(extractedRvUser, c.RvUser, c.Message.Chat.Type, App.DefaultRightVision, token);

            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        private async Task BanCommand(CommandContext c, CancellationToken token)
        {
            var (targetRvUser, message, reason, endDate) = await PrepareRestrictionAsync(c, PunishmentType.Ban, token);

            await Bot.Client.SendTextMessageAsync(c.Message.Chat, message, cancellationToken: token);

            if (targetRvUser != null)
            {
                await Bot.Client.BanChatMemberAsync(c.Message.Chat, targetRvUser.UserId, endDate, cancellationToken: token);

                targetRvUser.Punishments.Add(new(PunishmentType.Ban, c.Message.Chat.Id, reason, DateTime.Now, endDate));
            }
        }

        private async Task MuteCommand(CommandContext c, CancellationToken token = default)
        {

            var (targetRvUser, message, reason, endDate) = await PrepareRestrictionAsync(c, PunishmentType.Mute, token);

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

                targetRvUser.Punishments.Add(new(PunishmentType.Mute, c.Message.Chat.Id, reason, DateTime.Now, endDate));
            }
        }

        private async Task<(RvUser? targetRvUser, string message, string reason, DateTime endDate)> PrepareRestrictionAsync(CommandContext c, PunishmentType punishmentType, CancellationToken token = default)
        {
            var (extractedRvUser, args) = CommandFormatHelper.ExtractRvUserFromArgs(c);


            string message;
            if (extractedRvUser == null || extractedRvUser == c.RvUser || c.RvUser.Role <= extractedRvUser.Role)
            {
                message = extractedRvUser == null
                    ? "Пользователь не указан или не найден!"
                    : extractedRvUser == c.RvUser
                        ? "Извини, но ты не можешь выдать наказание самому себе!"
                        : "Извини, но ты не можешь выдать наказание пользователю, должность которого выше твоей!";

                await Bot.Client.SendTextMessageAsync(c.Message.Chat, message, cancellationToken: token);

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

            string reason = args.Length > 0 ? string.Join(" ", args) : Language.Phrases[extractedRvUser.Lang].Profile.Punishments.Punishment.NoReason ?? "Не указано";

            var endDate = DateTime.Now.AddMinutes(minutes);

            (string restrictionTypeString, string translatedRestrictionType, string whoGaveRestrictionString) = punishmentType switch
            {
                PunishmentType.Ban => ("бан", Language.Phrases[extractedRvUser.Lang].Profile.Punishments.Punishment.Ban, "Забанил"),
                PunishmentType.Mute => ("мут", Language.Phrases[extractedRvUser.Lang].Profile.Punishments.Punishment.Mute, "Замутил"),
                _ => (string.Empty, string.Empty, string.Empty)
            };

            message = $"Пользователь {extractedRvUser.Name} получает {restrictionTypeString} в группе!\n\n- {whoGaveRestrictionString}: {c.RvUser.Name} (@{c.RvUser.Telegram})\n- По причине: {reason}\n- До: {endDate.ToString("g", new CultureInfo("ru-RU"))}";


            var group = c.Message.Chat.Id switch
            {
                Bot.CriticChatId => Language.Phrases[extractedRvUser.Lang].Profile.Punishments.Punishment.InCritics,
                Bot.ParticipantChatId => Language.Phrases[extractedRvUser.Lang].Profile.Punishments.Punishment.InParticipants,
                _ => string.Empty
            };

            await Bot.Client.SendTextMessageAsync(
                extractedRvUser.UserId,
                string.Format(
                    Language.Phrases[extractedRvUser.Lang].Profile.Punishments.Notification,
                    extractedRvUser.Name,
                    translatedRestrictionType,
                    group,
                    endDate.ToString("g", new CultureInfo("ru-RU")),
                    reason) + Language.Phrases[extractedRvUser.Lang].Profile.Punishments.Contacts,
                cancellationToken: token);

            return (extractedRvUser, message, reason, endDate);
        }

        private async Task PermissionsBackCallback(CallbackContext c, CancellationToken token = default)
        {
            var targetRvUser = c.CallbackQuery.Message!.ReplyToMessage == null
            ? c.RvUser
            : c.DbContext.RvUsers.FirstOrDefault(u => u.UserId == c.CallbackQuery.Message!.ReplyToMessage.From!.Id);

            if (targetRvUser == null)
            {
                await Bot.Client.SendTextMessageAsync(c.CallbackQuery.Message.Chat, Language.Phrases[c.RvUser.Lang].Messages.Common.UserNotFound, cancellationToken: token);
                return;
            }

            (string message, InlineKeyboardMarkup? keyboard) = await ProfileHelper.Profile(targetRvUser, c.RvUser, c.CallbackQuery.Message.Chat.Type, App.DefaultRightVision, token);

            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        private async Task PermissionsCallback(CallbackContext c, CancellationToken token = default)
        {
            var targetUserId = long.Parse(c.CallbackQuery.Data!.Replace("permissions_minimized-", ""));
            await LocationsFront.PermissionsList(c, c.DbContext.RvUsers.First(u => u.UserId == targetUserId), c.CallbackQuery.Data!.Contains("minimized"), token);
        }

        private async Task CriticTakeCallback(CallbackContext c, CancellationToken token = default)
        {
            var args = c.CallbackQuery.Data!.Split('-');
            var userId = long.Parse(args.Last());

            var form = await c.DbContext.CriticForms.FirstOrDefaultAsync(c => c.UserId == userId, token);

            await HandleCuratorshipAsync(c.CallbackQuery, form, token);
        }

        private async Task ParticipantTakeCallback(CallbackContext c, CancellationToken token = default)
        {
            var args = c.CallbackQuery.Data!.Split('-');
            var userId = long.Parse(args.Last());

            var form = await c.RvContext.ParticipantForms.FirstOrDefaultAsync(c => c.UserId == userId, token);

            await HandleCuratorshipAsync(c.CallbackQuery, form, token);
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
                        c.DbContext.Remove(form);
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

        #region Handle methods

        private async Task HandleCuratorshipAsync(CallbackQuery callback, IForm? form, CancellationToken token = default)
        {
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
                "Bronze"
                or "Silver"
                or "Gold"
                or "Brilliant" =>
                (
                    string.Format(formHandlerMessages.FormAccepted, form.Name, Language.GetCategoryString(Enum.Parse<Category>(callbackType)), $"{c.CallbackQuery.From.FirstName}"),
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

        #endregion

        #endregion

    }
}
