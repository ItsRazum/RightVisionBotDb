using Microsoft.Extensions.Configuration;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Text.Sections;
using Serilog;

namespace RightVisionBotDb.Text
{
    public static class Phrases
    {
        public static Dictionary<Lang, LangInstance> Lang { get; } = [];

        public static void Build(params Lang[] langs)
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
                Lang.Add(l, langInstance ?? throw new NullReferenceException(nameof(langInstance)));
            }

            Lang.Add(Enums.Lang.Na, Lang[Enums.Lang.Ru]);

            App.RegisteredLangs = langs;
        }

        public static string GetUserStatusString(Status s, Lang lang) =>
            (string)Lang[lang].Profile.StatusHeaders
            .GetType()
            .GetProperty(s.ToString())?
            .GetValue(Lang[lang].Profile.StatusHeaders)!
            ?? throw new NullReferenceException(nameof(s));

        public static string GetUserRoleString(Role r, Lang lang) =>
            (string)Lang[lang].Profile.Roles
            .GetType()
            .GetProperty(r.ToString())?
            .GetValue(Lang[lang].Profile.Roles)!
            ?? throw new NullReferenceException(nameof(r));

        public static string GetFormStatusString(FormStatus s, Lang lang) =>
            (string)Lang[lang].Profile.Forms.Status
            .GetType()
            .GetProperty(s.ToString())?
            .GetValue(Lang[lang].Profile.Forms.Status)!
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

        public static string GetGroupTypeString(long groupId, Lang lang) => groupId switch
        {
            Constants.GroupId.CriticGroupId => Lang[lang].Profile.Punishments.Punishment.InCritics,
            Constants.GroupId.ParticipantGroupId => Lang[lang].Profile.Punishments.Punishment.InParticipants,
            Constants.GroupId.AcademyGeneralGroupId => Lang[lang].Profile.Punishments.Punishment.InAcademyGeneralChat,
            _ => string.Empty
        };

        public static string GetGroupLink(long groupId) => groupId switch
        {
            Constants.GroupId.CriticGroupId => Constants.Links.CriticGroupLink,
            Constants.GroupId.ParticipantGroupId => Constants.Links.ParticipantGroupLink,
            Constants.GroupId.AcademyGeneralGroupId => Constants.Links.AcademyGeneralGroup,
            _ => string.Empty
        };
    }
}
