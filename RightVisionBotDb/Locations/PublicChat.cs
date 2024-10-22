using RightVisionBotDb.Lang;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Locations
{
    internal sealed class PublicChat : RvLocation
    {

        #region Properties

        private ProfileStringService ProfileStringService { get; set; }

        #endregion

        #region Constructor

        public PublicChat(
            Bot bot,
            Keyboards keyboards,
            LocationManager locationManager,
            RvLogger logger,
            LogMessages logMessages,
            LocationsFront locationsFront,
            ProfileStringService profileStringService)
            : base(bot, keyboards, locationManager, logger, logMessages, locationsFront)
        {
            ProfileStringService = profileStringService;

            this
                .RegisterTextCommand("/profile", ProfileCommand)
                .RegisterTextCommand("/ban", BanCommand)
                .RegisterCallbackCommand("permissions_minimized", PermissionsMinimizedCallback)
                .RegisterCallbackCommand("permissions_maximized", PermissionsMaximizedCallback)
                .RegisterCallbackCommand("permissions_back", PermissionsBackCallback);
        }

        #endregion

        #region Methods

        private async Task ProfileCommand(CommandContext c, CancellationToken token)
        {
            var targetRvUser = c.Message.ReplyToMessage == null
                ? c.RvUser
                : c.DbContext.RvUsers.FirstOrDefault(u => u.UserId == c.Message.ReplyToMessage.From!.Id);

            (string message, InlineKeyboardMarkup? keyboard) =
                targetRvUser != null
                ? (ProfileStringService.Public(targetRvUser, c.RvUser, App.DefaultRightVision), await Keyboards.Profile(targetRvUser, c.Message.Chat.Type, App.DefaultRightVision, c.RvUser.Lang))
                : (Language.Phrases[c.RvUser.Lang].Messages.Common.UserNotFound, null);

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

            (string message, InlineKeyboardMarkup? keyboard) =
                targetRvUser != null
                ? (ProfileStringService.Public(targetRvUser, c.RvUser, App.DefaultRightVision), await Keyboards.Profile(targetRvUser, c.CallbackQuery.Message!.Chat.Type, App.DefaultRightVision, c.RvUser.Lang))
                : (Language.Phrases[c.RvUser.Lang].Messages.Common.UserNotFound, null);

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

        #endregion
    }
}
