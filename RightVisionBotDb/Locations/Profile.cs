using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    internal sealed class Profile : RvLocation
    {

        #region Constructor

        public Profile(
            Bot bot,
            LocationManager locationManager,
            RvLogger logger,
            LocationsFront locationsFront)
            : base(bot, locationManager, logger, locationsFront)
        {
            this
                .RegisterCallbackCommand("back", BackCallback)
                .RegisterCallbackCommand("mainmenu", MainMenuCallback)
                .RegisterCallbackCommand("forms", FormsCallback)
                .RegisterCallbackCommand("permissions_minimized", PermissionsMinimized)
                .RegisterCallbackCommand("permissions_maximized", PermissionsMaximized)
                .RegisterCallbackCommand("permissions_back", BackCallback)
                .RegisterCallbackCommand("criticForm", CriticFormCallback);
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
            using var rvdb = DatabaseHelper.GetRightVisionContext(App.DefaultRightVision);

            if (rvdb.Status == RightVisionStatus.Irrelevant)
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

        private async Task CriticFormCallback(CallbackContext c, CancellationToken token = default)
        {
            CriticForm criticForm;
            c.RvUser.Location = LocationManager[nameof(CriticFormLocation)];
            if (c.DbContext.CriticForms.FirstOrDefault(cf => cf.UserId == c.RvUser.UserId) != null)
                criticForm = c.DbContext.CriticForms.First(cf => cf.UserId == c.RvUser.UserId);

            else
            {
                criticForm = new CriticForm(c.RvUser.UserId, c.CallbackQuery.From.Username ?? string.Empty);
                c.DbContext.CriticForms.Add(criticForm);
            }
            if (criticForm.GetEmptyProperty(out var property))
                await LocationsFront.CriticForm(c, criticForm.GetPropertyStep(property!.Name), token);

        }

        #endregion
    }
}
