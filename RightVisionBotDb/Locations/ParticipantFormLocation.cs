using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Text;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    public sealed class ParticipantFormLocation : RvLocation
    {

        #region Properties

        public ParticipantFormService ParticipantFormService { get; }

        #endregion

        #region Constructor

        public ParticipantFormLocation(
            Bot bot,
            LocationService locationService,
            RvLogger logger,
            LocationsFront locationsFront,
            ParticipantFormService participantFormService)
            : base(bot, locationService, logger, locationsFront)
        {
            ParticipantFormService = participantFormService;

            this.RegisterTextCommand("«", BackCommand);
        }

        #endregion

        #region RvLocation overrides

        public override async Task HandleCommandAsync(CommandContext c, bool containsArgs, CancellationToken token = default)
        {
            await base.HandleCommandAsync(c, containsArgs, token);

            if (c.Message.Text!.StartsWith('«')) return;

            var form = c.RvContext.ParticipantForms.FirstOrDefault(form => form.UserId == c.RvUser.UserId);
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
                            await Bot.Client.SendTextMessageAsync(c.Message.Chat, Phrases.Lang[c.RvUser.Lang].Messages.Common.EnterAnInteger, cancellationToken: token);
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
                    await Bot.Client.SendTextMessageAsync(c.Message.Chat, Phrases.Lang[c.RvUser.Lang].Messages.Common.EnterAnInteger, cancellationToken: token);
                }
            }
        }

        #endregion

        #region Methods

        private async Task BackCommand(CommandContext c, CancellationToken token = default)
        {
            var form = c.RvContext.ParticipantForms.FirstOrDefault(form => form.UserId == c.RvUser.UserId);
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

        private async Task CheckFormCompletion(CommandContext c, ParticipantForm form, CancellationToken token)
        {
            if (form.GetEmptyProperty(out var property))
            {
                if (ParticipantFormService.Messages.TryGetValue(form.GetPropertyStep(property!.Name), out var getMessage))
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
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, Phrases.Lang[c.RvUser.Lang].Messages.Participant.FormSubmitted, replyMarkup: KeyboardsHelper.ReplyMainMenu, cancellationToken: token);
                await Bot.Client.SendTextMessageAsync(-1001968408177,
                    $"Пришла новая заявка на участие!\n\n" +
                    $"Имя: {form.Name}\n" +
                    $"Тег: {form.Telegram}\n" +
                    $"Ссылка на канал: {form.Link}\n" +
                    $"Субъективная оценка навыков: {form.Rate}\n",
                    replyMarkup: KeyboardsHelper.TakeCuratorship(form),
                    cancellationToken: token);

                c.RvUser.UserPermissions -= Permission.SendParticipantForm;
                form.Status = FormStatus.Waiting;
            }
        }

        #endregion
    }
}
