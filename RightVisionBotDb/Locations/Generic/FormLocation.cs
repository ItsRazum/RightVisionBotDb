using EasyForms.Types;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Text;
using RightVisionBotDb.Types;
using System.Reflection;
using Telegram.Bot;

namespace RightVisionBotDb.Locations.Generic
{
    public abstract class FormLocation<TForm, TFormService> : RvLocation 
        where TForm : Form, IForm 
        where TFormService : IFormService
    {

        #region Properties

        protected TFormService FormService { get; }

        #endregion

        #region Contructor

        protected FormLocation(
            Bot bot, 
            LocationService locationService, 
            RvLogger logger, 
            LocationsFront locationsFront,
            TFormService formService) 
            : base(bot, locationService, logger, locationsFront)
        {
            FormService = formService;
            this.RegisterTextCommand("«", BackCommand);
        }

        #endregion

        #region Methods

        #region Overridable

        protected abstract Task<TForm> GetUserFormAsync(CommandContext c, CancellationToken token = default);

        protected abstract Task OnFormCompleted(CommandContext c, TForm form, CancellationToken token);

        protected virtual Task<bool> ValidateIntPropertyAsync(PropertyInfo property, int value, CommandContext c, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        protected virtual Task<bool> ValidateStringPropertyAsync(PropertyInfo property, string value, CommandContext c, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        protected virtual async Task CheckFormCompletion(CommandContext c, TForm form, CancellationToken token)
        {
            if (form.GetEmptyProperty(out var property))
            {
                var step = form.GetPropertyStep(property!.Name);
                if (FormService.Messages.TryGetValue(step, out var getMessage))
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
                await OnFormCompleted(c, form, token);
            }
        }

        protected virtual async Task<bool> TrySetPropertyValue(CommandContext c, TForm form, PropertyInfo property, string inputValue, CancellationToken token)
        {
            var propType = property.PropertyType;

            object? parsedValue = inputValue;

            if (propType == typeof(int))
            {
                if (!int.TryParse(inputValue, out var intVal))
                {
                    await Bot.Client.SendTextMessageAsync(c.Message.Chat, Phrases.Lang[c.RvUser.Lang].Messages.Common.EnterAnInteger, cancellationToken: token);
                    return false;
                }

                if (!await ValidateIntPropertyAsync(property, intVal, c, token))
                    return false;

                parsedValue = intVal;
            }
            else if (propType == typeof(string))
            {
                if (!await ValidateStringPropertyAsync(property, inputValue, c, token))
                    return false;
            }

            try
            {
                form.SetPropertyValue(property.Name, parsedValue!);
            }
            catch (ArgumentException)
            {
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, Phrases.Lang[c.RvUser.Lang].Messages.Common.EnterAnInteger, cancellationToken: token);
                return false;
            }

            return true;
        }

        #endregion

        private async Task BackCommand(CommandContext c, CancellationToken token = default)
        {
            var form = await GetUserFormAsync(c, token);
            if (form == null) return;

            if (form.GetEmptyProperty(out var property))
            {
                var targetStep = form.GetPropertyStep(property!.Name) - 1;
                if (targetStep > 0)
                {
                    var targetProperty = form.GetProperty(targetStep);
                    if (targetProperty.PropertyType == typeof(int))
                        form.SetPropertyValue(targetStep, 0);
                    else if (targetProperty.PropertyType == typeof(string))
                        form.SetPropertyValue(targetStep, string.Empty);

                    await CheckFormCompletion(c, form, token);
                }
            }
        }

        #endregion
    }
}
