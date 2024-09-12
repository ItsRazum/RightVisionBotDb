using RightVisionBotDb.Data;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Locations
{
    internal sealed class MainMenu : RvLocationBase, IRvLocation
    {

        #region Properties

        public ProfileStringService Profile { get; }

        #endregion

        #region Constructor

        public MainMenu(
            Bot bot,
            Keyboards inlineKeyboards,
            LocationManager locationManager,
            RvLogger logger,
            LogMessages logMessages,
            ProfileStringService profile,
            LocationsFront locationsFront)
            : base(bot, inlineKeyboards, locationManager, logger, logMessages, locationsFront)
        {
            Profile = profile;
        }

        #endregion

        #region IRvLocation implementation

        public async Task HandleCallbackAsync(CallbackQuery callbackQuery, RvUser rvUser, ApplicationDbContext context, CancellationToken token)
        {
            if (rvUser != null)
            {
                switch (callbackQuery.Data)
                {
                    case "mainmenu":
                        await Bot.Client.EditMessageTextAsync(
                            callbackQuery.Message!.Chat,
                            callbackQuery.Message.MessageId,
                            string.Format(Language.Phrases[rvUser.Lang].Messages.Common.Greetings, rvUser.Name),
                            replyMarkup: InlineKeyboards.Hub(rvUser),
                            cancellationToken: token);
                        break;
                    case "about":
                        await Bot.Client.EditMessageTextAsync(
                            callbackQuery.Message!.Chat,
                            callbackQuery.Message.MessageId,
                            Language.Phrases[rvUser.Lang].Messages.Common.About,
                            replyMarkup: InlineKeyboards.About(rvUser),
                            cancellationToken: token);
                        break;
                    case "profile":
                        await LocationsFront.Profile(rvUser, callbackQuery, context, token);
                        break;
                }
            }
        }

        public Task HandleCommandAsync(Message message, RvUser rvUser, ApplicationDbContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        #endregion

    }
}
