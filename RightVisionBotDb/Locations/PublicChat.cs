using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Lang;
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

        #region Constructor

        public PublicChat(
            Bot bot,
            LocationManager locationManager,
            RvLogger logger,
            LocationsFront locationsFront)
            : base(bot, locationManager, logger, locationsFront)
        {
            this
                .RegisterTextCommand("/profile", ProfileCommand)
                .RegisterTextCommand("/ban", BanCommand, Permission.Ban)
                .RegisterCallbackCommand("permissions_minimized", PermissionsMinimizedCallback)
                .RegisterCallbackCommand("permissions_maximized", PermissionsMaximizedCallback)
                .RegisterCallbackCommand("permissions_back", PermissionsBackCallback)
                .RegisterCallbackCommand("c_take", CriticTakeCallback, Permission.Curate)
                .RegisterCallbackCommand("c_form", CriticFormCallback);
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
                    await Bot.Client.SendTextMessageAsync(c.Message.Chat, "Слишком много аргументов в команде!");
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

        private async Task PermissionsMinimizedCallback(CallbackContext c, CancellationToken token = default)
        {
            var targetUserId = long.Parse(c.CallbackQuery.Data!.Replace("permissions_minimized-", ""));
            await LocationsFront.PermissionsList(c, c.DbContext.RvUsers.First(u => u.UserId == targetUserId), true, token);
        }

        private async Task PermissionsMaximizedCallback(CallbackContext c, CancellationToken token = default)
        {
            var targetUserId = long.Parse(c.CallbackQuery.Data!.Replace("permissions_maximized-", ""));
            await LocationsFront.PermissionsList(c, c.DbContext.RvUsers.First(u => u.UserId == targetUserId), false, token);
        }

        private async Task CriticTakeCallback(CallbackContext c, CancellationToken token = default)
        {
            var args = c.CallbackQuery.Data!.Split('-');
            var userId = long.Parse(args.Last());
            var callback = c.CallbackQuery;

            var form = await c.DbContext.CriticForms.FirstOrDefaultAsync(c => c.UserId == userId, token);

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

        private async Task CriticFormCallback(CallbackContext c, CancellationToken token = default)
        {
            var args = c.CallbackQuery.Data!.Split('-');
            var callbackType = args[1];
            var userId = long.Parse(args.Last());
            var callback = c.CallbackQuery;

            var form = c.DbContext.CriticForms.First(c => c.UserId == userId);

            if (form != null)
            {
                if (c.RvUser.UserId != form.CuratorId) return;

                var rvUser = c.DbContext.RvUsers.First(u => u.UserId == userId);
                var formattedDate = DateTime.Now.ToString("g", new CultureInfo("ru-RU"));
                var criticMessages = Language.Phrases[rvUser.Lang].Messages.Critic;
                var messageText = callback.Message!.Text;

                (string messageForSender, string formText, FormStatus formStatus, InlineKeyboardMarkup? keyboard) = callbackType switch
                {
                    "deny" =>
                    (
                        string.Format(criticMessages.FormDenied, form.Name, $"{c.CallbackQuery.From.Username}"),
                        $"{messageText}\n[{formattedDate}] ❌Заявка отклонена!",
                        FormStatus.Denied,
                        null
                    ),
                    "requestPM" =>
                    (
                        string.Format(criticMessages.PMRequested, form.Name, $"@{c.CallbackQuery.From.Username}"),
                        $"{messageText}\n[{formattedDate}] 📩Запрошено личное сообщение",
                        FormStatus.Waiting,
                        KeyboardsHelper.CandidateOptions(form)
                    ),
                    "reset" =>
                    (
                        string.Format(criticMessages.FormCanceled, form.Name, $"{c.CallbackQuery.From.FirstName}"),
                        $"{messageText}\n[{formattedDate}] ⚠️Заявка сброшена!",
                        FormStatus.Reset,
                        null
                    ),
                    "Bronze" 
                    or "Silver" 
                    or "Gold" 
                    or "Brilliant" =>
                    (
                        string.Format(criticMessages.FormAccepted, form.Name, Language.GetCategoryString(Enum.Parse<Category>(callbackType)), $"{c.CallbackQuery.From.FirstName}"),
                        $"{messageText}\n[{formattedDate}] ✅Заявка принята! Категория: {Enum.Parse<Category>(callbackType)}",
                        FormStatus.Accepted,
                        null
                    ),
                    _ => throw new InvalidDataException(nameof(c.CallbackQuery.Data))
                };
                try
                {
                    await Bot.Client.SendTextMessageAsync(
                        userId,
                        messageForSender,
                        replyMarkup: KeyboardsHelper.ReplyMainMenu,
                        cancellationToken: token);

                    form.Status = formStatus;

                    switch (formStatus)
                    {
                        case FormStatus.Accepted:
                            form.Category = Enum.Parse<Category>(callbackType);
                            break;
                        case FormStatus.Reset:
                            c.DbContext.CriticForms.Remove(form);
                            c.RvUser.UserPermissions += Permission.SendCriticForm;
                            break;
                    }

                    await Bot.Client.EditMessageTextAsync(
                        callback.Message!.Chat,
                        callback.Message.MessageId,
                        formText,
                        replyMarkup: keyboard,
                        cancellationToken: token);
                }
                catch (Exception ex)
                {
                    await Bot.Client.SendTextMessageAsync(
                        callback.Message!.Chat,
                        $"Во время обработки заявки пользователя @{rvUser.Telegram} ({rvUser.UserId}) произошла ошибка!\n{ex.Message}");

                    throw;
                }
            }
            else
                await Bot.Client.EditMessageTextAsync(
                    callback.Message!.Chat,
                    callback.Message.MessageId,
                    "Ошибка: заявки не существует.",
                    cancellationToken: token);
        }

        #endregion
    }
}
