using RightVisionBotDb.Helpers;
using RightVisionBotDb.Lang;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Services
{
    public class ParticipantFormService
    {

        #region Properties

        public Dictionary<int, Func<Enums.Lang, (string message, ReplyKeyboardMarkup? keyboard)>> Messages { get; }

        #endregion

        #region Constructor

        public ParticipantFormService()
        {
            Messages = new()
            {
                { 1, lang => (Language.Phrases[lang].Messages.Participant.EnterName, KeyboardsHelper.ReplyBack(lang)) },
                { 2, lang => (Language.Phrases[lang].Messages.Participant.EnterLink, null) },
                { 3, lang => (Language.Phrases[lang].Messages.Participant.EnterRate, KeyboardsHelper.RateSelection(lang)) },
                { 4, lang => (Language.Phrases[lang].Messages.Participant.EnterTrack, KeyboardsHelper.ReplyBack(lang)) }
            };
        }

        #endregion

    }
}
