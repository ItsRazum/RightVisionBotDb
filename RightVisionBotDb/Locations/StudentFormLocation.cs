using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Locations.Generic;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Text;
using RightVisionBotDb.Types;
using System.Reflection;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    internal class StudentFormLocation : FormLocation<StudentForm, StudentFormService>
    {
        #region Constructor

        public StudentFormLocation(
            Bot bot, 
            LocationService locationService,
            RvLogger logger, 
            LocationsFront locationsFront,
            StudentFormService studentFormService) 
            : base(bot, locationService, logger, locationsFront, studentFormService)
        {
        }

        #endregion

        #region FormLocation overrides

        protected override async Task<StudentForm> GetUserFormAsync(CommandContext c, CancellationToken token = default)
        {
            return await c.AcademyContext.StudentForms.FirstAsync(st => st.UserId == c.RvUser.UserId, token);
        }

        protected override async Task<bool> ValidateIntPropertyAsync(PropertyInfo property, int value, CommandContext c, CancellationToken token)
        {
            if (property.Name == nameof(CriticForm.Rate) && (value < 1 || value > 4))
            {
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, Phrases.Lang[c.RvUser.Lang].Messages.Common.EnterAnInteger, cancellationToken: token);
                return false;
            }
            return true;
        }

        protected override async Task OnFormCompleted(CommandContext c, StudentForm form, CancellationToken token)
        {
            await Bot.Client.SendTextMessageAsync(c.Message.Chat,
                Phrases.Lang[c.RvUser.Lang].Messages.Academy.FormSubmitted,
                replyMarkup: KeyboardsHelper.ReplyMainMenu,
                cancellationToken: token);

            await Bot.Client.SendTextMessageAsync(-1001968408177,
                $"Пришла новая заявка на вступление в академию!\n\n" +
                $"Имя: {form.Name}\n" +
                $"Тег: @{form.Telegram}\n" +
                $"Ссылка на канал: {form.Link}\n" +
                $"Субъективная оценка навыков: {form.Rate}\n",
                replyMarkup: KeyboardsHelper.TakeCuratorship(form),
                cancellationToken: token);

            c.RvUser.UserPermissions -= Permission.SendStudentForm;
            form.Status = FormStatus.Waiting;
        }

        protected override async Task CancelFormAsync(CommandContext c, StudentForm form, CancellationToken token = default)
        {
            c.AcademyContext.StudentForms.Remove(form);
            await LocationsFront.MainMenu(c, token);
        }

        #endregion

    }
}
