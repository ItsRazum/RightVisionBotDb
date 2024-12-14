using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Services
{
    public class StudentFormService : IFormService
    {
        public Dictionary<int, Func<Lang, (string message, ReplyKeyboardMarkup? keyboard)>> Messages { get; }

        public StudentFormService() 
        {
            Messages = new()
            {
                { 1, lang => (Phrases.Lang[lang].Messages.Academy.EnterName, KeyboardsHelper.ReplyBack(lang)) },
                { 2, lang => (Phrases.Lang[lang].Messages.Academy.EnterLink, null) },
                { 3, lang => (Phrases.Lang[lang].Messages.Academy.EnterRate, KeyboardsHelper.RateSelection(lang)) },
            };
        }
    }
}
