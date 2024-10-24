using RightVisionBotDb.Helpers;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    internal sealed class Start : RvLocation
    {

        #region Constructor

        public Start(
            Bot bot,
            LocationManager locationManager,
            RvLogger logger,
            LocationsFront locationsFront)
            : base(bot, locationManager, logger, locationsFront)
        {
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
            await RvLogger.Log(LogMessagesHelper.Registration(rvUser), rvUser, token);
        }

        #endregion
    }
}
