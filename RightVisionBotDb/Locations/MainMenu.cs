﻿using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Text;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    public sealed class MainMenu : RvLocation
    {

        #region Constructor

        public MainMenu(
            Bot bot,
            LocationService locationService,
            RvLogger logger,
            LocationsFront locationsFront)
            : base(bot, locationService, logger, locationsFront)
        {
            this
                .RegisterCallbackCommand("back", MainMenuCallback)
                .RegisterCallbackCommand("mainmenu", MainMenuCallback)
                .RegisterCallbackCommand("about", AboutCallback)
                .RegisterCallbackCommand("aboutBot", AboutBotCallback)
                .RegisterCallbackCommand("forms", FormsCallback)
                .RegisterCallbackCommand("academy", AcademyCallback)
                .RegisterCallbackCommand("criticForm", CriticFormCallback, Permission.SendCriticForm)
                .RegisterCallbackCommand("participantForm", ParticipantFormCallback, Permission.SendParticipantForm);
        }

        #endregion

        #region Methods

        private async Task MainMenuCallback(CallbackContext c, CancellationToken token = default)
        {
            await LocationsFront.MainMenu(c, token);
        }

        private async Task AboutCallback(CallbackContext c, CancellationToken token = default)
        {
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                Phrases.Lang[c.RvUser.Lang].Messages.Common.About,
                replyMarkup: KeyboardsHelper.About(c.RvUser.Lang),
                cancellationToken: token);
        }

        private async Task AboutBotCallback(CallbackContext c, CancellationToken token = default)
        {
            await Bot.Client.EditMessageTextAsync(
                c.CallbackQuery.Message!.Chat,
                c.CallbackQuery.Message.MessageId,
                string.Format(Phrases.Lang[c.RvUser.Lang].Messages.Common.AboutBot, App.Configuration.BotSettings.BuildDate),
                disableWebPagePreview: true,
                replyMarkup: KeyboardsHelper.AboutBot(c.RvUser.Lang),
                cancellationToken: token);
        }

        private async Task FormsCallback(CallbackContext c, CancellationToken token = default)
        {
            using var rvdb = DatabaseHelper.GetRightVisionContext(App.DefaultRightVision);

            if (rvdb.Status == RightVisionStatus.Irrelevant)
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

        private async Task AcademyCallback(CallbackContext c, CancellationToken token = default)
        {
            if (Bot.Parameters.HasFlag(BotParameters.EnableAcademy))
            {
                var (content, keyboard) = AcademyHelper.MainMenu(c);
                await Bot.Client.EditMessageTextAsync(
                    c.CallbackQuery.Id,
                    content,
                    replyMarkup: keyboard,
                    cancellationToken: token);
            }

            else
                await Bot.Client.AnswerCallbackQueryAsync(
                    c.CallbackQuery.Id,
                    Phrases.Lang[c.RvUser.Lang].Messages.Academy.EnrollmentClosed,
                    true,
                    cancellationToken: token);
        }

        private async Task CriticFormCallback(CallbackContext c, CancellationToken token = default)
        {
            c.RvUser.Location = LocationService[nameof(CriticFormLocation)];
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
            c.RvUser.Location = LocationService[nameof(ParticipantFormLocation)];
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
