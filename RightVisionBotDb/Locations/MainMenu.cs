using RightVisionBotDb.Helpers;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    internal sealed class MainMenu : RvLocation
    {

        public MainMenu(
            Bot bot,
            LocationManager locationManager,
            RvLogger logger,
            LocationsFront locationsFront)
            : base(bot, locationManager, logger, locationsFront)
        {
            this
                .RegisterCallbackCommand("back", MainMenuCallback)
                .RegisterCallbackCommand("mainmenu", MainMenuCallback)
                .RegisterCallbackCommand("about", AboutCallback)
                .RegisterCallbackCommand("profile", ProfileCallback)
                .RegisterCallbackCommand("forms", FormsCallback);
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
                replyMarkup: KeyboardsHelper.InlineBack(c.RvUser.Lang),
                cancellationToken: token);
        }

        private async Task ProfileCallback(CallbackContext c, CancellationToken token)
        {
            await LocationsFront.Profile(c, token);
        }

        private async Task FormsCallback(CallbackContext c, CancellationToken token = default)
        {
            using var rvdb = DatabaseHelper.GetRightVisionContext(App.DefaultRightVision);

            if (rvdb.Status == Enums.RightVisionStatus.Irrelevant)
            {
                await Bot.Client.AnswerCallbackQueryAsync(
                    c.CallbackQuery.Id,
                    Language.Phrases[c.RvUser.Lang].Messages.Common.EnrollmentClosed,
                    showAlert: true,
                    cancellationToken: token);
            }
            else
            {
                await LocationsFront.FormSelection(c, token);
            }
        }
    }
}
