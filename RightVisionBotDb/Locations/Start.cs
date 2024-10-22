using DryIoc;
using RightVisionBotDb.Data;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Locations
{
    internal sealed class Start : RvLocation
    {

        #region Constructor

        public Start(
            Bot bot,
            Keyboards keyboards,
            LocationManager locationManager,
            RvLogger logger,
            LogMessages logMessages,
            LocationsFront locationsFront)
            : base(bot, keyboards, locationManager, logger, logMessages, locationsFront)
        {
            RegisterTextCommand("/start", StartCommand);

            this
                .RegisterCallbackCommand("Ru", LangCallback)
                .RegisterCallbackCommand("Ua", LangCallback)
                .RegisterCallbackCommand("Kz", LangCallback);
        }

        #endregion

        #region Methods

        private async Task LangCallback(CallbackContext c, CancellationToken token = default)
        {
            var rvUser = c.RvUser;
            rvUser.Lang = Enum.Parse<Enums.Lang>(c.CallbackQuery.Data!);
            await LocationsFront.MainMenu(c, token);
            await RvLogger.Log(LogMessages.Registration(rvUser), rvUser, token);
        }

        private async Task StartCommand(CommandContext c, CancellationToken token = default)
        {
            await Bot.Client.SendTextMessageAsync(c.Message.Chat, "Choose Lang:", replyMarkup: Keyboards.СhooseLang, cancellationToken: token);
        }

        #endregion
    }
}
