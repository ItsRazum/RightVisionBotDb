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
    public sealed class CriticFormLocation : FormLocation<CriticForm, CriticFormService>
    {

        #region Constructor

        public CriticFormLocation(
            Bot bot,
            LocationService locationService,
            RvLogger logger,
            LocationsFront locationsFront,
            CriticFormService criticFormService)
            : base(bot, locationService, logger, locationsFront, criticFormService)
        {
        }

        #endregion

        #region FormLocation overrides

        protected override async Task<bool> ValidateIntPropertyAsync(PropertyInfo property, int value, CommandContext c, CancellationToken token)
        {
            if (property.Name == nameof(CriticForm.Rate) && (value < 1 || value > 4))
            {
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, Phrases.Lang[c.RvUser.Lang].Messages.Common.EnterAnInteger, cancellationToken: token);
                return false;
            }
            return true;
        }

        protected override async Task<bool> ValidateStringPropertyAsync(PropertyInfo property, string value, CommandContext c, CancellationToken token)
        {
            if (property.Name == nameof(CriticForm.Link) && !value.StartsWith("https://"))
            {
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, Phrases.Lang[c.RvUser.Lang].Messages.Critic.IncorrectFormat, cancellationToken: token);
                return false;
            }
            return true;
        }

        protected override async Task<CriticForm> GetUserFormAsync(CommandContext c, CancellationToken token = default)
        {
            return await c.DbContext.CriticForms.FirstAsync(cf => cf.UserId == c.RvUser.UserId, token);
        }

        protected override async Task OnFormCompleted(CommandContext c, CriticForm form, CancellationToken token)
        {
            await Bot.Client.SendTextMessageAsync(c.Message.Chat,
                Phrases.Lang[c.RvUser.Lang].Messages.Critic.FormSubmitted,
                replyMarkup: KeyboardsHelper.ReplyMainMenu,
                cancellationToken: token);

            await Bot.Client.SendTextMessageAsync(-1001968408177,
                $"Пришла новая заявка на должность судьи!\n\n" +
                $"Имя: {form.Name}\n" +
                $"Тег: @{form.Telegram}\n" +
                $"Ссылка на канал: {form.Link}\n" +
                $"Субъективная оценка навыков: {form.Rate}\n" +
                $"Что написал о себе: {form.AboutYou}\n" +
                $"Почему мы должны его принять: {form.WhyYou}\n",
                replyMarkup: KeyboardsHelper.TakeCuratorship(form),
                cancellationToken: token);

            c.RvUser.UserPermissions -= Permission.SendCriticForm;
            form.Status = FormStatus.Waiting;
        }

        protected override async Task CancelFormAsync(CommandContext c, CriticForm form, CancellationToken token = default)
        {
            c.DbContext.CriticForms.Remove(form);
            await LocationsFront.MainMenu(c, token);
        }

        #endregion

    }
}
