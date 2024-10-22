using RightVisionBotDb.Lang;
using RightVisionBotDb.Permissions;
using RightVisionBotDb.Services;
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
            Keyboards keyboards,
            LocationManager locationManager,
            RvLogger logger,
            LogMessages logMessages,
            LocationsFront locationsFront,
            CriticFormService criticFormService)
            : base(bot, keyboards, locationManager, logger, logMessages, locationsFront)
        {
            CriticFormService = criticFormService;

            foreach (var lang in App.RegisteredLangs)
            {
                RegisterTextCommand(Language.Phrases[lang].KeyboardButtons.Back, BackCommand);
            }
        }



        public override async Task HandleCommandAsync(CommandContext c, bool containsArgs, CancellationToken token = default)
        {
            await base.HandleCommandAsync(c, containsArgs, token);

            var form = c.DbContext.CriticForms.FirstOrDefault(form => form.UserId == c.RvUser.UserId);
            if (form == null)
            {
                await Bot.Client.SendTextMessageAsync(c.Message.Chat, "Произошла ошибка, вернись в главное меню", cancellationToken: token);
            }

            else
            {
                if (form.GetEmptyProperty(out var property))
                {
                    if (property!.Name == "Rate")
                    {
                        if (int.TryParse(c.Message.Text!, out var rate))
                            form.SetPropertyValue(property.Name, rate);

                        else
                        {
                            await Bot.Client.SendTextMessageAsync(c.Message.Chat, Language.Phrases[c.RvUser.Lang].Messages.Common.EnterAnInteger, cancellationToken: token);
                            return;
                        }
                    }
                    else
                        form.SetPropertyValue(property.Name, c.Message.Text);

                    if (form.GetEmptyProperty(out property))
                    {
                        if (CriticFormService.Messages.TryGetValue(form.GetPropertyStep(property!.Name), out var getMessage))
                            await Bot.Client.SendTextMessageAsync(c.Message.Chat, getMessage(c.RvUser.Lang), cancellationToken: token);
                    }
                    else
                    {
                        await Bot.Client.SendTextMessageAsync(c.Message.Chat, Language.Phrases[c.RvUser.Lang].Messages.Critic.FormSubmitted, cancellationToken: token);
                        await Bot.Client.SendTextMessageAsync(-1001968408177,
                            $"Пришла новая заявка на должность судьи!\n\n" +
                            $"Имя: {form.Name}\n" +
                            $"Тег: {form.Telegram}\n" +
                            $"Ссылка на канал: {form.Link}\n" +
                            $"Субъективная оценка навыков: {form.Rate}\n" +
                            $"Что написал о себе: {form.AboutYou}\n" +
                            $"Почему мы должны его принять: {form.WhyYou}\n",
                            replyMarkup: Keyboards.CriticCuratorship(form.UserId),
                            cancellationToken: token);

                        c.RvUser.Permissions -= Permission.SendCriticForm;
                        form.Status = Enums.FormStatus.Waiting;
                    }
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

                if (property.PropertyType == typeof(int))
                    form.SetPropertyValue(targetPropertyStep, 0);

                else if (property.PropertyType == typeof(string))
                    form.SetPropertyValue(targetPropertyStep, "0");


                if (form.GetEmptyProperty(out property))
                {
                    if (form.GetPropertyStep(property!.Name) == -1)
                    {
                        c.DbContext.CriticForms.Remove(form);
                        c.RvUser.Location = LocationManager[nameof(MainMenu)];
                        await Bot.Client.SendTextMessageAsync(
                            c.Message.Chat,
                            Language.Phrases[c.RvUser.Lang].Messages.Common.SendFormRightNow,
                            replyMarkup: Keyboards.FormSelection(c.RvUser),
                            cancellationToken: token);
                    }
                    else
                    {
                        if (CriticFormService.Messages.TryGetValue(form.GetPropertyStep(property.Name), out var getMessage))
                            await Bot.Client.SendTextMessageAsync(c.Message.Chat, getMessage(c.RvUser.Lang), cancellationToken: token);
                    }
                }
                else
                {
                    await Bot.Client.SendTextMessageAsync(c.Message.Chat, Language.Phrases[c.RvUser.Lang].Messages.Critic.FormSubmitted, cancellationToken: token);
                    await Bot.Client.SendTextMessageAsync(-1001968408177,
                        $"Пришла новая заявка на должность судьи!\n\n" +
                        $"Имя: {form.Name}\n" +
                        $"Тег: {form.Telegram}\n" +
                        $"Ссылка на канал: {form.Link}\n" +
                        $"Субъективная оценка навыков: {form.Rate}\n" +
                        $"Что написал о себе: {form.AboutYou}\n" +
                        $"Почему мы должны его принять: {form.WhyYou}\n",
                        replyMarkup: Keyboards.CriticCuratorship(form.UserId),
                        cancellationToken: token);

                    c.RvUser.Permissions -= Permission.SendCriticForm;
                    form.Status = Enums.FormStatus.Waiting;
                }
            }
        }

        #endregion
    }
}
