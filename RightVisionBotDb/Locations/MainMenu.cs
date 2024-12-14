﻿using EasyForms.Types;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Interfaces;
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
                .RegisterCallbackCommand("participantForm", ParticipantFormCallback, Permission.SendParticipantForm)
                .RegisterCallbackCommand("studentForm", StudentFormCallback, Permission.SendStudentForm);
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
            using var rvdb = DatabaseHelper.GetRightVisionContext(App.Configuration.RightVisionSettings.DefaultRightVision);

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
                    c.CallbackQuery.Message!.Chat,
                    c.CallbackQuery.Message.MessageId,
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

        private async Task<bool> HandleFormAsync<TForm>(
            CallbackContext c,
            IQueryable<TForm> formsQuery,
            Func<long, string?, TForm> createFormFunc,
            Func<CallbackContext, int, CancellationToken, Task> showFormStepFunc,
            Action<TForm> addFormAction,
            CancellationToken token = default)
            where TForm : Form, IForm
        {
            var result = false;
            var form = formsQuery.FirstOrDefault(f => f.UserId == c.RvUser.UserId);
            if (form == null)
            {
                form = createFormFunc(c.RvUser.UserId, c.CallbackQuery.From.Username);
                addFormAction(form);
                result = true;
            }

            if (form.GetEmptyProperty(out var property))
            {
                var step = form.GetPropertyStep(property!.Name);
                await showFormStepFunc(c, step, token);
            }

            return result;
        }


        private async Task CriticFormCallback(CallbackContext c, CancellationToken token = default)
        {
            await HandleFormAsync(
                c,
                c.DbContext.CriticForms,
                (userId, username) => new CriticForm(userId, username),
                LocationsFront.CriticForm,
                f => c.DbContext.CriticForms.Add(f),
                token);

        }

        private async Task ParticipantFormCallback(CallbackContext c, CancellationToken token = default)
        {
            await HandleFormAsync(
                c,
                c.RvContext.ParticipantForms,
                (userId, username) => new ParticipantForm(userId, username),
                LocationsFront.ParticipantForm,
                f => c.RvContext.ParticipantForms.Add(f),
                token);
        }

        private async Task StudentFormCallback(CallbackContext c, CancellationToken token)
        {
            await HandleFormAsync(
                c,
                c.AcademyContext.StudentForms,
                (userId, username) => new StudentForm(userId, username),
                LocationsFront.StudentForm,
                f => c.AcademyContext.StudentForms.Add(f),
                token);
        }

        #endregion

    }
}
