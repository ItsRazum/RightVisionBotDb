using EasyForms.Types;
using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
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
            LocationService locationService,
            RvLogger logger,
            LocationsFront locationsFront)
            : base(bot, locationService, logger, locationsFront)
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
            await HandleFormAsync(
                c,
                LocationService[nameof(CriticFormLocation)],
                c => c.DbContext.CriticForms,
                c => new CriticForm(c.RvUser.UserId, c.CallbackQuery.From.Username),
                LocationsFront.CriticForm,
                token);
        }

        private async Task ParticipantFormCallback(CallbackContext c, CancellationToken token = default)
        {
            await HandleFormAsync(
                c,
                LocationService[nameof(ParticipantFormLocation)],
                c => c.RvContext.ParticipantForms,
                c => new ParticipantForm(c.RvUser.UserId, c.CallbackQuery.From.Username),
                LocationsFront.ParticipantForm,
                token);
        }

        private async Task HandleFormAsync<TForm>(
            CallbackContext c,
            RvLocation location,
            Func<CallbackContext, DbSet<TForm>> getDbSet,
            Func<CallbackContext, TForm> createForm,
            Func<CallbackContext, int, CancellationToken, Task> locationFrontMethod,
            CancellationToken token = default
            ) where TForm : Form, IForm
        {
            c.RvUser.Location = location;

            var dbSet = getDbSet(c);

            var form = await dbSet.FirstOrDefaultAsync(f => c.RvUser.Is(f), token);
            if (form == null)
            {
                form = createForm(c);
                await dbSet.AddAsync(form, token);
            }

            if (form.GetEmptyProperty(out var property))
            {
                var step = form.GetPropertyStep(property!.Name);
                await locationFrontMethod(c, step, token);
            }
        }

        private async Task SendingCallback(CallbackContext c, CancellationToken token = default)
        {
            var phrases = Phrases.Lang[c.RvUser.Lang];
            (c.RvUser.UserPermissions, string callbackAnswer, string buttonMessage) = c.RvUser.Has(Permission.Sending)
                ? (c.RvUser.UserPermissions - Permission.Sending, phrases.Messages.Profile.Sending.UnsubscribeSuccess, phrases.KeyboardButtons.Sending.Subscribe)
                : (c.RvUser.UserPermissions + Permission.Sending, phrases.Messages.Profile.Sending.SubscribeSuccess,   phrases.KeyboardButtons.Sending.Unsubscribe);

            (string profileText, InlineKeyboardMarkup? _) = await ProfileHelper.Profile(c.RvUser, c, c.CallbackQuery.Message!.Chat.Type, App.Configuration.RightVisionSettings.DefaultRightVision, false, token);

            var newInlineKeyboard = new InlineKeyboardMarkup(c.CallbackQuery.Message!.ReplyMarkup!.InlineKeyboard.Select(row => 
            row.Select(button => button.CallbackData == "sending"
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
