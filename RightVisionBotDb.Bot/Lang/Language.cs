using Microsoft.Extensions.Configuration;
using RightVisionBotDb.Bot.Lang.Phrases;
using RightVisionBotDb.Enums;
using Serilog;

namespace RightVisionBotDb.Bot.Lang
{
    public class Language
    {
        public static Dictionary<Enums.Lang, LangInstance> Phrases = new();
        public static void Build(IConfiguration configuration, params Enums.Lang[] lang)
        {
            foreach (var l in lang)
            {
                Log.Logger?.Information($"Сборка {l} языка...");

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

        public static string GetCategoryString(Category c) =>
            c switch
            {
                Category.Bronze => "🥉Bronze",
                Category.Silver => "🥈Silver",
                Category.Gold => "🥇Gold",
                Category.Brilliant => "💎Brilliant",
                _ => string.Empty
            };  
    }
}
