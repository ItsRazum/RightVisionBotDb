using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RightVisionBotDb.Data;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Lang.Interfaces;
using RightVisionBotDb.Models;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;
using System.Globalization;
using System.Linq.Expressions;
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
            var targetRvUser = c.Message.ReplyToMessage == null
                ? c.RvUser
                : c.DbContext.RvUsers.FirstOrDefault(u => u.UserId == c.Message.ReplyToMessage.From!.Id);

            if (targetRvUser == null)
            {
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, Language.Phrases[c.RvUser.Lang].Messages.Common.UserNotFound, cancellationToken: token);
                return;
            }
            (string message, InlineKeyboardMarkup? keyboard) = await ProfileHelper.Profile(targetRvUser, c.RvUser, c.Message.Chat.Type, App.DefaultRightVision, token);

            await Bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                message,
                replyMarkup: keyboard,
                cancellationToken: token);
        }

        private async Task BanCommand(CommandContext c, CancellationToken token)
        {
            var commandArgs = c.Message.Text!.Trim().Split(' ');
            switch (commandArgs.Length)
            {
                case 1:
                    if (c.Message.ReplyToMessage == null)
                    {
                        await Bot.Client.SendTextMessageAsync(c.Message.Chat, "Данная команда используется только по отношению к другому пользователю!", cancellationToken: token);
                        return;
                    }
                    else
                    {
                        await Bot.Client.SendTextMessageAsync(c.Message.Chat, "");
                        break;
                    }
                case 2:
                    break;
                case 3:
                    break;
                default:
                    await Bot.Client.SendTextMessageAsync(c.Message.Chat, "Слишком много аргументов в команде!", cancellationToken: token);
                    break;
            }
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
