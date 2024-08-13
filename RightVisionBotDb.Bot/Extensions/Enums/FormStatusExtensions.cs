using RightVisionBotDb.Bot.Lang;
using RightVisionBotDb.Enums;

namespace RightVisionBotDb.Bot.Extensions.Enums
{
    internal static class FormStatusExtensions
    {
        public static string ToString(this FormStatus status, RightVisionBotDb.Enums.Lang lang)
        {
            return status switch
            {
                FormStatus.Blocked => Language.Phrases[lang].Profile.Forms.Status.Blocked,
                FormStatus.Waiting => Language.Phrases[lang].Profile.Forms.Status.Waiting,
                FormStatus.NotFinished => Language.Phrases[lang].Profile.Forms.Status.NotFinished,
                FormStatus.Accepted => Language.Phrases[lang].Profile.Forms.Status.Accepted,
                FormStatus.UnderConsideration => Language.Phrases[lang].Profile.Forms.Status.UnderConsideration,
                FormStatus.Denied => Language.Phrases[lang].Profile.Forms.Status.Denied,
                _ => Language.Phrases[lang].Profile.Forms.Status.CouldNotGet
            };
        }
    }
}
