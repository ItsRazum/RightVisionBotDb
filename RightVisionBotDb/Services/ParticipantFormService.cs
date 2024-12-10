using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Services
{
    public class ParticipantFormService : IFormService
    {

        #region Properties

        public Dictionary<int, Func<Lang, (string message, ReplyKeyboardMarkup? keyboard)>> Messages { get; }

        #endregion

        #region Constructor

        public ParticipantFormService()
        {
            Messages = new()
            {
                { 1, lang => (Phrases.Lang[lang].Messages.Participant.EnterName, KeyboardsHelper.ReplyBack(lang)) },
                { 2, lang => (Phrases.Lang[lang].Messages.Participant.EnterLink, null) },
                { 3, lang => (Phrases.Lang[lang].Messages.Participant.EnterRate, KeyboardsHelper.RateSelection(lang)) },
                { 4, lang => (Phrases.Lang[lang].Messages.Participant.EnterTrack, KeyboardsHelper.ReplyBack(lang)) }
            };
        }

        #endregion

    }
}
