using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Permissions;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    internal sealed class CriticForm : RvLocation
    {

        #region Properties

        private static readonly Dictionary<int, Func<Enums.Lang, string>> Messages = new()
        {
            { 1, lang => Language.Phrases[lang].Messages.Critic.EnterName },
            { 2, lang => Language.Phrases[lang].Messages.Critic.EnterLink },
            { 3, lang => Language.Phrases[lang].Messages.Critic.EnterRate },
            { 4, lang => Language.Phrases[lang].Messages.Critic.EnterAboutYou },
            { 5, lang => Language.Phrases[lang].Messages.Critic.EnterWhyYou }
        };

        #endregion

        public CriticForm(
            Bot bot,
            Keyboards keyboards,
            LocationManager locationManager,
            RvLogger logger,
            LogMessages logMessages,
            LocationsFront locationsFront)
            : base(bot, keyboards, locationManager, logger, logMessages, locationsFront)
        {
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
                var property = form.GetEmptyProperty();
                if (property != null)
                {
                    if (property.Name == "Rate")
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

                    property = form.GetEmptyProperty();
                    if (property != null)
                    {
                        if (Messages.TryGetValue(form.GetPropertyStep(property.Name), out var getMessage))
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

            var property = form.GetEmptyProperty();

            if (property != null)
            {
                var targetPropertyStep = form.GetPropertyStep(property.Name) - 1;

                if (property.PropertyType == typeof(int))
                    form.SetPropertyValue(targetPropertyStep, 0);

                else if (property.PropertyType == typeof(string))
                    form.SetPropertyValue(targetPropertyStep, "0");


                property = form.GetEmptyProperty();

                if (property != null)
                {
                    if (form.GetPropertyStep(property.Name) == -1)
                    {
                        c.DbContext.CriticForms.Remove(form);
                        c.RvUser.Location = LocationManager["MainMenu"];
                        await Bot.Client.SendTextMessageAsync(
                            c.Message.Chat,
                            Language.Phrases[c.RvUser.Lang].Messages.Common.SendFormRightNow,
                            replyMarkup: Keyboards.FormSelection(c.RvUser),
                            cancellationToken: token);
                    }
                    else
                    {
                        if (Messages.TryGetValue(form.GetPropertyStep(property.Name), out var getMessage))
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
