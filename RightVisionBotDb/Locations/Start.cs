﻿using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;

namespace RightVisionBotDb.Locations
{
    internal sealed class Start : RvLocation
    {

        #region Constructor

        public Start(
            Bot bot,
            LocationService locationService,
            RvLogger logger,
            LocationsFront locationsFront)
            : base(bot, locationService, logger, locationsFront)
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
            rvUser.Lang = Enum.Parse<Lang>(c.CallbackQuery.Data!);
            await LocationsFront.MainMenu(c, token);
            await RvLogger.Log(LogMessagesHelper.Registration(rvUser), rvUser, token);
        }

        #endregion
    }
}
