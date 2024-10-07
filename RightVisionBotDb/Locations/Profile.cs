using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;

namespace RightVisionBotDb.Locations
{
    internal sealed class Profile : RvLocationBase, IRvLocation
    {

        #region Constructor

        public Profile(
            Bot bot,
            Keyboards keyboards,
            LocationManager locationManager,
            RvLogger logger,
            LogMessages logMessages,
            LocationsFront locationsFront,
            ProfileStringService profileStringService)
            : base(bot, keyboards, locationManager, logger, logMessages, locationsFront)
        {
            this
                .RegisterCallbackCommand("back", BackCallback)
                .RegisterCallbackCommand("mainmenu", MainMenuCallback)
                .RegisterCallbackCommand("forms", FormsCallback)
                .RegisterCallbackCommand("permissions_minimized", PermissionsMinimized)
                .RegisterCallbackCommand("permissions_maximized", PermissionsMaximized)
                .RegisterCallbackCommand("permissions_back", BackCallback);
        }

        #endregion

        #region Methods

        private async Task BackCallback(CallbackContext c, CancellationToken token = default)
        {
            await LocationsFront.Profile(c, token);
        }

        private async Task MainMenuCallback(CallbackContext c, CancellationToken token = default)
        {
            await LocationsFront.MainMenu(c, token);
        }

        private async Task FormsCallback(CallbackContext c, CancellationToken token = default)
        {
            await LocationsFront.FormSelection(c, token);
        }

        private async Task PermissionsMinimized(CallbackContext c, CancellationToken token = default)
        {
            var targetUserId = long.Parse(c.CallbackQuery.Data!.Replace("permissions_minimized-", ""));
            await LocationsFront.PermissionsList(c, c.DbContext.RvUsers.First(u => u.UserId == targetUserId), true, token);
        }

        private async Task PermissionsMaximized(CallbackContext c, CancellationToken token = default)
        {
            var targetUserId = long.Parse(c.CallbackQuery.Data!.Replace("permissions_maximized-", ""));
            await LocationsFront.PermissionsList(c, c.DbContext.RvUsers.First(u => u.UserId == targetUserId), false, token);
        }

        #endregion
    }
}
