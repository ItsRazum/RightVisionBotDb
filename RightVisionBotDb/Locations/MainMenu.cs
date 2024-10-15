using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    internal sealed class MainMenu : RvLocation
    {
        #region Properties

        private DatabaseService DatabaseService { get; }

        #endregion


        public MainMenu(
            Bot bot,
            Keyboards keyboards,
            LocationManager locationManager,
            RvLogger logger,
            LogMessages logMessages,
            LocationsFront locationsFront,
            DatabaseService databaseService)
            : base(bot, keyboards, locationManager, logger, logMessages, locationsFront)
        {
            DatabaseService = databaseService;

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

        private async Task FormsCallback(CallbackContext c, CancellationToken token = default)
        {
            using var rvdb = DatabaseService.GetRightVisionContext(App.DefaultRightVision);

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
