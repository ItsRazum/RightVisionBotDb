using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    internal sealed class CriticFormLocation : RvLocation
    {

        #region Properties

        private CriticFormService CriticFormService { get; }

        #endregion

        public CriticFormLocation(
            Bot bot,
            LocationManager locationManager,
            RvLogger logger,
            LocationsFront locationsFront,
            CriticFormService criticFormService)
            : base(bot, locationManager, logger, locationsFront)
        {
            CriticFormService = criticFormService;

            this.RegisterTextCommand("«", BackCommand);
        }



        public override async Task HandleCommandAsync(CommandContext c, bool containsArgs, CancellationToken token = default)
        {
            await base.HandleCommandAsync(c, containsArgs, token);

            if (c.Message.Text!.StartsWith('«')) return;

            var form = c.DbContext.CriticForms.FirstOrDefault(form => form.UserId == c.RvUser.UserId);
            if (form == null)
            {
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, "Не удалось получить данные твоей заявки. Пожалуйста, вернись в главное меню: /menu", cancellationToken: token);
                return;
            }

            if (form.GetEmptyProperty(out var property))
            {
                try
                {
                    if (property!.Name == "Rate")
                    {
                        if (int.TryParse(c.Message.Text, out var rate) && rate <= 4 && rate >= 1)
                        {
                            form.SetPropertyValue(property.Name, rate);
                        }
                        else
                        {
                            await Bot.Client.SendTextMessageAsync(c.Message.Chat, Language.Phrases[c.RvUser.Lang].Messages.Common.EnterAnInteger, cancellationToken: token);
                            return;
                        }
                    }
                    else if (property.Name == "Link")
                    {
                        if (c.Message.Text.StartsWith("https://"))
                        {
                            form.SetPropertyValue(property.Name, c.Message.Text);
                        }
                        else
                        {
                            await Bot.Client.SendTextMessageAsync(c.Message.Chat, Language.Phrases[c.RvUser.Lang].Messages.Critic.IncorrectFormat, cancellationToken: token);
                            return;
                        }
                    }
                    else
                    {
                        form.SetPropertyValue(property.Name, c.Message.Text);
                    }
                    await CheckFormCompletion(c, form, token);
                }
                catch (ArgumentException)
                {
                    await Bot.Client.SendTextMessageAsync(c.Message.Chat, Language.Phrases[c.RvUser.Lang].Messages.Common.EnterAnInteger, cancellationToken: token);
                }
            }
        }

        #region Methods

        private async Task BackCommand(CommandContext c, CancellationToken token = default)
        {
            var form = c.DbContext.CriticForms.FirstOrDefault(form => form.UserId == c.RvUser.UserId);
            if (form == null) return;

            if (form.GetEmptyProperty(out var property))
            {
                var targetPropertyStep = form.GetPropertyStep(property!.Name) - 1;
                var targetProperty = form.GetProperty(targetPropertyStep);

                if (targetProperty.PropertyType == typeof(int))
                    form.SetPropertyValue(targetPropertyStep, 0);
                else if (targetProperty.PropertyType == typeof(string))
                    form.SetPropertyValue(targetPropertyStep, string.Empty);

                await CheckFormCompletion(c, form, token);
            }
        }

        private async Task CheckFormCompletion(CommandContext c, CriticForm form, CancellationToken token)
        {
            if (form.GetEmptyProperty(out var property))
            {
                if (CriticFormService.Messages.TryGetValue(form.GetPropertyStep(property!.Name), out var getMessage))
                {
                    var result = getMessage(c.RvUser.Lang);
                    var message = result.message.Contains('{')
                        ? string.Format(result.message, form.Name)
                        : result.message;

                    await Bot.Client.SendTextMessageAsync(c.Message.Chat, message, replyMarkup: result.keyboard, cancellationToken: token);
                }
            }
            else
            {
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, Language.Phrases[c.RvUser.Lang].Messages.Critic.FormSubmitted, replyMarkup: KeyboardsHelper.ReplyMainMenu, cancellationToken: token);
                await Bot.Client.SendTextMessageAsync(-1001968408177,
                    $"Пришла новая заявка на должность судьи!\n\n" +
                    $"Имя: {form.Name}\n" +
                    $"Тег: {form.Telegram}\n" +
                    $"Ссылка на канал: {form.Link}\n" +
                    $"Субъективная оценка навыков: {form.Rate}\n" +
                    $"Что написал о себе: {form.AboutYou}\n" +
                    $"Почему мы должны его принять: {form.WhyYou}\n",
                    replyMarkup: KeyboardsHelper.TakeCuratorship(form),
                    cancellationToken: token);

                c.RvUser.UserPermissions -= Permission.SendCriticForm;
                form.Status = FormStatus.Waiting;
            }
        }

        #endregion
    }
}
