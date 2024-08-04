using DryIoc;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Lang.Phrases;
using Microsoft.Extensions.Configuration;
using Serilog;

//система динамической мультиязычности
namespace RightVisionBotDb.Lang
{
    public class Language
    {
        public static Dictionary<Enums.Lang, LangInstance> Phrases = new();
        public static void Build(Enums.Lang[] lang)
        {
            foreach (var l in lang)
            {
                Log.Logger?.Information($"Сборка {l} языка...");
                var configuration = App.Container.Resolve<Bot>().Configuration;

                var langInstanceSection = configuration.GetSection("LangInstance");
                var langInstance = langInstanceSection.Get<LangInstance>();
                Phrases.Add(l, langInstance ?? throw new NullReferenceException(nameof(langInstance)));
            }

            Log.Logger?.Information("Готово.");
        }

        public static string GetUserStatusString(Status s, Enums.Lang lang) =>
            (string)Phrases[lang].Profile.StatusHeaders
            .GetType()
            .GetProperty(s.ToString())?
            .GetValue(Phrases[lang].Profile.StatusHeaders)! 
            ?? throw new NullReferenceException(nameof(s));

        public static string GetUserRoleString(Role r, Enums.Lang lang) => 
            (string)Phrases[lang].Profile.Roles
            .GetType()
            .GetProperty(r.ToString())?
            .GetValue(Phrases[lang].Profile.Roles)! 
            ?? throw new NullReferenceException(nameof(r));
    }
}
