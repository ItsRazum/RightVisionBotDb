using RightVisionBotDb.Helpers;
using RightVisionBotDb.Lang;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Singletons
{
    public class CriticFormService
    {

        #region Properties

        public Dictionary<int, Func<Enums.Lang, (string message, ReplyKeyboardMarkup? keyboard)>> Messages { get; }

        #endregion

        #region Constructor

        public CriticFormService()
        {
            Messages = new()
            {
                { 1, lang => (Language.Phrases[lang].Messages.Critic.EnterName, KeyboardsHelper.ReplyBack(lang)) },
                { 2, lang => (Language.Phrases[lang].Messages.Critic.EnterLink, null) },
                { 3, lang => (Language.Phrases[lang].Messages.Critic.EnterRate, KeyboardsHelper.RateSelection(lang)) },
                { 4, lang => (Language.Phrases[lang].Messages.Critic.EnterAboutYou, KeyboardsHelper.ReplyBack(lang)) },
                { 5, lang => (Language.Phrases[lang].Messages.Critic.EnterWhyYou, null) }
            };
        }

        #endregion
    }

}