using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Services
{
    public class CriticFormService : IFormService
    {

        #region Properties

        public Dictionary<int, Func<Lang, (string message, ReplyKeyboardMarkup? keyboard)>> Messages { get; }

        #endregion

        #region Constructor

        public CriticFormService()
        {
            Messages = new()
            {
                { 1, lang => (Phrases.Lang[lang].Messages.Critic.EnterName, KeyboardsHelper.ReplyBack(lang)) },
                { 2, lang => (Phrases.Lang[lang].Messages.Critic.EnterLink, null) },
                { 3, lang => (Phrases.Lang[lang].Messages.Critic.EnterRate, KeyboardsHelper.RateSelection(lang)) },
                { 4, lang => (Phrases.Lang[lang].Messages.Critic.EnterAboutYou, KeyboardsHelper.ReplyBack(lang)) },
                { 5, lang => (Phrases.Lang[lang].Messages.Critic.EnterWhyYou, null) }
            };
        }

        #endregion
    }
}