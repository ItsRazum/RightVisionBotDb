using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Models;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Text;
using RightVisionBotDb.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Locations
{
    public sealed class Profile : RvLocation
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
                .RegisterCallbackCommand("criticForm", CriticFormCallback, Permission.SendCriticForm)
                .RegisterCallbackCommand("participantForm", ParticipantFormCallback, Permission.SendParticipantForm)
                .RegisterCallbackCommand("sending", SendingCallback);
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
                    Phrases.Lang[c.RvUser.Lang].Messages.Common.EnrollmentClosed,
                    showAlert: true,
                    cancellationToken: token);
            }
            else
            {
                await LocationsFront.FormSelection(c, token);
            }
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

        private async Task SendingCallback(CallbackContext c, CancellationToken token = default)
        {
            var phrases = Phrases.Lang[c.RvUser.Lang];
            (c.RvUser.UserPermissions, string callbackAnswer, string buttonMessage) = c.RvUser.Has(Permission.Sending)
                ? (c.RvUser.UserPermissions - Permission.Sending, phrases.Messages.Profile.Sending.UnsubscribeSuccess, phrases.KeyboardButtons.Sending.Subscribe)
                : (c.RvUser.UserPermissions + Permission.Sending, phrases.Messages.Profile.Sending.SubscribeSuccess, phrases.KeyboardButtons.Sending.Unsubscribe);

            (string profileText, InlineKeyboardMarkup? _) = await ProfileHelper.Profile(c.RvUser, c, c.CallbackQuery.Message!.Chat.Type, App.DefaultRightVision, token, false);

            var newInlineKeyboard = new InlineKeyboardMarkup(c.CallbackQuery.Message!.ReplyMarkup!.InlineKeyboard.Select(row => row
            .Select(button => button.CallbackData == "sending"
                ? InlineKeyboardButton.WithCallbackData(buttonMessage, button.CallbackData)
                : button)
            .ToArray())
        .ToArray());

            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message.Chat,
                c.CallbackQuery.Message.MessageId,
                profileText,
                replyMarkup: newInlineKeyboard,
                cancellationToken: token);

            await Bot.Client.AnswerCallbackQueryAsync(c.CallbackQuery.Id, callbackAnswer, cancellationToken: token);
        }

        #endregion
    }
}
