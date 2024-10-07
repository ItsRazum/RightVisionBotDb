using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    internal sealed class MainMenu : RvLocationBase
    {
        public MainMenu(
            Bot bot,
            Keyboards keyboards,
            LocationManager locationManager,
            RvLogger logger,
            LogMessages logMessages,
            LocationsFront locationsFront)
            : base(bot, keyboards, locationManager, logger, logMessages, locationsFront)
        {
            this
                .RegisterCallbackCommand("back", BackCallback)
                .RegisterCallbackCommand("mainmenu", MainMenuCallback)
                .RegisterCallbackCommand("about", AboutCallback)
                .RegisterCallbackCommand("profile", ProfileCallback)
                .RegisterCallbackCommand("forms", FormsCallback);
        }

        private async Task BackCallback(CallbackContext c, CancellationToken token)
        {
            await LocationsFront.MainMenu(c, token);
        }

        private async Task MainMenuCallback(CallbackContext c, CancellationToken token)
        {
            await LocationsFront.MainMenu(c, token);
        }

        private async Task AboutCallback(CallbackContext c, CancellationToken token)
        {
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Language.Phrases[c.RvUser.Lang].Messages.Common.About,
                cancellationToken: token);
        }

        private async Task ProfileCallback(CallbackContext c, CancellationToken token)
        {
            await LocationsFront.Profile(c, token);
        }

        private async Task FormsCallback(CallbackContext c, CancellationToken token)
        {
            await LocationsFront.FormSelection(c, token);
        }
    }
}
