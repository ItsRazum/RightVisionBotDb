using RightVisionBotDb.Lang;

namespace RightVisionBotDb.Services
{
    public class CriticFormService
    {

        #region Properties

        public Dictionary<int, Func<Enums.Lang, string>> Messages { get; }

        #endregion

        #region Constructor

        public CriticFormService()
        {
            Messages = new()
            {
                { 1, lang => Language.Phrases[lang].Messages.Critic.EnterName },
                { 2, lang => Language.Phrases[lang].Messages.Critic.EnterLink },
                { 3, lang => Language.Phrases[lang].Messages.Critic.EnterRate },
                { 4, lang => Language.Phrases[lang].Messages.Critic.EnterAboutYou },
                { 5, lang => Language.Phrases[lang].Messages.Critic.EnterWhyYou }
            };
        }

        #endregion
    }

}