using Microsoft.Extensions.Configuration;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Lang.Phrases;
using Serilog;

namespace RightVisionBotDb.Lang
{
    public static class Language
    {
        public static Dictionary<Enums.Lang, LangInstance> Phrases { get; } = [];

        public static void Build(params Enums.Lang[] langs)
        {
            var languageConfiguration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory);

            foreach (var lang in langs.Prepend(Enums.Lang.Ru)) 
                languageConfiguration.AddJsonFile($"Resources/Lang/{lang}.json", false);
            
            var language = languageConfiguration.Build();

            foreach (var l in langs)
            {
                Log.Logger?.Information($"Сборка {l} языка...");

                var langInstanceSection = language.GetSection(l.ToString());
                var langInstance = langInstanceSection.Get<LangInstance>();
                Phrases.Add(l, langInstance ?? throw new NullReferenceException(nameof(langInstance)));
            }

            Phrases.Add(Enums.Lang.Na, Phrases[Enums.Lang.Ru]);

            App.RegisteredLangs = langs;
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

        public static string GetFormStatusString(FormStatus s, Enums.Lang lang) =>
            (string)Phrases[lang].Profile.Forms.Status
            .GetType()
            .GetProperty(s.ToString())?
            .GetValue(Phrases[lang].Profile.Forms.Status)!
            ?? throw new NullReferenceException(nameof(s));

        public static string GetCategoryString(Category c) =>
            c switch
            {
                Category.Bronze => "🥉Bronze",
                Category.Silver => "🥈Silver",
                Category.Gold => "🥇Gold",
                Category.Brilliant => "💎Brilliant",
                _ => string.Empty
            };

        public static string GetGroupTypeString(long groupId, Enums.Lang lang) => groupId switch
        {
            Bot.CriticChatId => Language.Phrases[lang].Profile.Punishments.Punishment.InCritics,
            Bot.ParticipantChatId => Language.Phrases[lang].Profile.Punishments.Punishment.InParticipants,
            _ => string.Empty
        };
    }
}
