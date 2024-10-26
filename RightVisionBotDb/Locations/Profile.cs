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
                .RegisterCallbackCommand("permissions_minimized", PermissionsCallback)
                .RegisterCallbackCommand("permissions_maximized", PermissionsCallback)
                .RegisterCallbackCommand("permissions_back", BackCallback)
                .RegisterCallbackCommand("criticForm", CriticFormCallback)
                .RegisterCallbackCommand("participantForm", ParticipantFormCallback);
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
            if (c.RvContext.Status == RightVisionStatus.Irrelevant)
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

        private async Task PermissionsCallback(CallbackContext c, CancellationToken token = default)
        {
            var targetUserId = long.Parse(c.CallbackQuery.Data!.Replace("permissions_minimized-", ""));
            await LocationsFront.PermissionsList(c, c.DbContext.RvUsers.First(u => u.UserId == targetUserId), c.CallbackQuery.Data!.Contains("minimized"), token);
        }

        private async Task CriticFormCallback(CallbackContext c, CancellationToken token = default)
        {
            c.RvUser.Location = LocationManager[nameof(CriticFormLocation)];
            var form = c.DbContext.CriticForms.FirstOrDefault(cf => cf.UserId == c.RvUser.UserId);
            if (form == null)
            {
                form = new CriticForm(c.RvUser.UserId, c.CallbackQuery.From.Username);
                c.DbContext.CriticForms.Add(form);
            }

            if (form.GetEmptyProperty(out var property))
                await LocationsFront.CriticForm(c, form.GetPropertyStep(property!.Name), token);
        }

        private async Task ParticipantFormCallback(CallbackContext c, CancellationToken token = default)
        {
            c.RvUser.Location = LocationManager[nameof(ParticipantFormLocation)];
            var form = c.RvContext.ParticipantForms.FirstOrDefault(pf => pf.UserId == c.RvUser.UserId);
            if (form == null)
            {
                form = new ParticipantForm(c.RvUser.UserId, c.CallbackQuery.From.Username);
                c.RvContext.ParticipantForms.Add(form);
            }
            if (form.GetEmptyProperty(out var property))
                await LocationsFront.ParticipantForm(c, form.GetPropertyStep(property!.Name), token);
        }
 
        #endregion
    }
}
