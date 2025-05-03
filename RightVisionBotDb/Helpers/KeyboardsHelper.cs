using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;
using RightVisionBotDb.Text;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Helpers
{
    public static class KeyboardsHelper
    {

        #region Inline keyboards

        public static InlineKeyboardMarkup СhooseLang => new(
        [
            [
                InlineKeyboardButton.WithCallbackData("🇷🇺RU / CIS", Lang.Ru.ToString())
            ],
            [
                InlineKeyboardButton.WithCallbackData("🇺🇦UA", Lang.Ua.ToString()),
                InlineKeyboardButton.WithCallbackData("🇰🇿KZ", Lang.Kz.ToString())
            ]
        ]);

        public static InlineKeyboardMarkup MainMenu(RvUser rvUser) => new(
        [
            [
                InlineKeyboardButton.WithCallbackData(Phrases.Lang[rvUser.Lang].KeyboardButtons.About, "about"),
                InlineKeyboardButton.WithCallbackData(Phrases.Lang[rvUser.Lang].KeyboardButtons.Apply, "forms")
            ],
            [
                InlineKeyboardButton.WithCallbackData(Phrases.Lang[rvUser.Lang].KeyboardButtons.Academy, "academy")
            ],
            [
                InlineKeyboardButton.WithCallbackData(Phrases.Lang[rvUser.Lang].KeyboardButtons.MyProfile, $"profile-{rvUser.UserId}")
            ]
        ]);

        public static InlineKeyboardMarkup About(Lang lang) => new(
[
            InlineKeyboardButton.WithCallbackData(Phrases.Lang[lang].KeyboardButtons.Back, "back"),
            InlineKeyboardButton.WithCallbackData(Phrases.Lang[lang].KeyboardButtons.AboutBot, "aboutBot")
        ]);

        public static InlineKeyboardButton InlineBack(Lang lang) =>
                InlineKeyboardButton.WithCallbackData(Phrases.Lang[lang].KeyboardButtons.Back, "back");

        public static InlineKeyboardMarkup AboutBot(Lang lang) =>
            new[]
            {
                InlineKeyboardButton.WithCallbackData(Phrases.Lang[lang].KeyboardButtons.Back, "about")
            };

        public static async Task<InlineKeyboardMarkup> Profile(RvUser rvUser, ChatType type, string rightvision, Lang lang)
        {
            var userId = rvUser.UserId;
            var rightVisions = App.AllRightVisions;
            var isOldParticipant = false;
            string? firstParticipationRightVision = null;
            var phrases = Phrases.Lang[lang];

            foreach (var rightvisionName in rightVisions)
            {
                using var rvdb = DatabaseHelper.GetRightVisionContext(rightvisionName);
                var form = await rvdb.ParticipantForms
                    .FirstOrDefaultAsync(
                        p => p.UserId == userId && p.Status == FormStatus.Accepted);

                if (form != null)
                {
                    isOldParticipant = true;
                    firstParticipationRightVision = rightvisionName;
                    break;
                }
            }

            List<InlineKeyboardButton[]> keyboardLayers =
            [
                [
                    InlineKeyboardButton.WithCallbackData(rightvision, $"rvProperties-{rightvision}")
                ]
            ];

            if (isOldParticipant)
                keyboardLayers.Add(
                    [
                        InlineKeyboardButton.WithCallbackData(phrases.KeyboardButtons.Participations, $"participations-{rvUser.UserId}-{firstParticipationRightVision ?? rightvision}")
                    ]);

            List<InlineKeyboardButton> keyboardButtons = [];

            keyboardLayers.Add(
                [
                    InlineKeyboardButton.WithCallbackData(phrases.KeyboardButtons.PermissionsList, $"permissions_minimized-{rvUser.UserId}"),
                    InlineKeyboardButton.WithCallbackData(phrases.KeyboardButtons.PunishmentsHistory, $"punishments_history-{rvUser.UserId}")
                ]);

            if (type != ChatType.Private) return keyboardLayers.ToArray();

            keyboardLayers.Add([
                InlineKeyboardButton.WithCallbackData(phrases.KeyboardButtons.Apply, "forms")
            ]);

            keyboardLayers.Add([
                InlineKeyboardButton.WithCallbackData(phrases.KeyboardButtons.Back, "mainmenu")
            ]);

            string subscribeContent = !rvUser.Has(Permission.Sending)
                ? phrases.KeyboardButtons.Sending.Subscribe
                : phrases.KeyboardButtons.Sending.Unsubscribe;

            keyboardLayers.Add([
                InlineKeyboardButton.WithCallbackData(subscribeContent, "sending")
            ]);


            if (rvUser.Has(Permission.TrackCard))
                keyboardLayers.Add([
				InlineKeyboardButton.WithCallbackData(phrases.KeyboardButtons.EditTrack, "m_edittrack"),
				InlineKeyboardButton.WithCallbackData(phrases.KeyboardButtons.SendTrack, "m_trackcard"),
				//InlineKeyboardButton.WithCallbackData(phrases.KeyboardButtons.GetVisual, "getvisual")
            ]);

            if (rvUser.Has(Permission.CriticMenu))
                keyboardLayers.Add(
                    [
                        InlineKeyboardButton.WithCallbackData(phrases.KeyboardButtons.CriticMenu.Open, "opencriticmenu")
                    ]);

            return keyboardLayers.ToArray();
        }

        public static InlineKeyboardMarkup FormSelection(RvUser rvUser)
        {
            var buttons = new List<InlineKeyboardButton[]>();

            var backButton = InlineKeyboardButton.WithCallbackData(
                Phrases.Lang[rvUser.Lang].KeyboardButtons.Back,
                "back"
            );

            var upperFloor = new List<InlineKeyboardButton>();

            if (rvUser.Has(Permission.SendParticipantForm))
                upperFloor.Add(InlineKeyboardButton.WithCallbackData(
                    Phrases.Lang[rvUser.Lang].KeyboardButtons.ParticipantFormVariationOne,
                    "participantForm"
                    )
                );

            if (rvUser.Has(Permission.SendCriticForm))
                upperFloor.Add(InlineKeyboardButton.WithCallbackData(
                    Phrases.Lang[rvUser.Lang].KeyboardButtons.CriticFormVariationOne,
                    "criticForm"
                    )
                );

            buttons.Add([.. upperFloor]);
            buttons.Add([backButton]);

            return new InlineKeyboardMarkup(buttons);
        }

        public static InlineKeyboardMarkup UserParticipations(RvUser rvUser, Lang lang)
        {
            return InlineKeyboardButton.WithCallbackData(Phrases.Lang[lang].KeyboardButtons.Back, $"profile-{rvUser.UserId}");
        }

        public static InlineKeyboardMarkup PermissionsList(RvUser rvUser, bool minimize, bool showAdvancedOptions, Lang lang)
        {
            List<InlineKeyboardButton> buttons = [];
            if (showAdvancedOptions)
            {
                var buttonValues = minimize
                ? (Phrases.Lang[lang].KeyboardButtons.Maximize, $"permissions_maximized-{rvUser.UserId}")
                : (Phrases.Lang[lang].KeyboardButtons.Minimize, $"permissions_minimized-{rvUser.UserId}");

                buttons.Add(InlineKeyboardButton.WithCallbackData(buttonValues.Item1, buttonValues.Item2));
            }

            buttons.Add(InlineKeyboardButton.WithCallbackData(Phrases.Lang[lang].KeyboardButtons.Back, $"profile-{rvUser.UserId}"));

            return new(buttons);
        }

        public static InlineKeyboardMarkup PunishmentsList(RvUser rvUser, bool bansCheckBoxEnabled, bool mutesCheckBoxEnabled, Lang lang)
        {
            var backButton = InlineKeyboardButton.WithCallbackData(Phrases.Lang[lang].KeyboardButtons.Back, $"profile-{rvUser.UserId}");

            if (rvUser.Punishments.Count == 0)
                return new(backButton);

            static (string checkBox, string callback) getButtonParts(bool condition, PunishmentType punishmentType, long userId)
            {
                (string checkBox, string prefix) = condition
                    ? ("✓", "hide")
                    : ("+", "show");
                return (checkBox, $"punishments_{prefix}-{punishmentType}-{userId}");
            }

            List<InlineKeyboardButton[]> layers = [];

            if (rvUser.Punishments.Any(p => p.Type == PunishmentType.Ban) &&
                rvUser.Punishments.Any(p => p.Type == PunishmentType.Mute)) //true, если у пользователя в наказаниях имеются как бан, так и мут
            {
                var (banCheckBox, banCallback) = getButtonParts(bansCheckBoxEnabled, PunishmentType.Ban, rvUser.UserId);
                layers.Add([InlineKeyboardButton.WithCallbackData(
                    banCheckBox + " " + Phrases.Lang[rvUser.Lang].Profile.Punishments.Punishment.Buttons.ShowBans,
                    banCallback)]
                    );

                var (muteCheckBox, muteCallback) = getButtonParts(mutesCheckBoxEnabled, PunishmentType.Mute, rvUser.UserId);
                layers.Add([InlineKeyboardButton.WithCallbackData(
                    muteCheckBox + " " + Phrases.Lang[rvUser.Lang].Profile.Punishments.Punishment.Buttons.ShowMutes,
                    muteCallback)]
                    );
            }

            layers.Add([backButton]);

            return new(layers);
        }

        public static InlineKeyboardMarkup TakeCuratorship(IForm form)
        {
            return new(InlineKeyboardButton.WithCallbackData($"Взять кураторство над заявкой", $"{form.CallbackType}take-{form.UserId}"));
        }

        public static InlineKeyboardMarkup CandidateOptions(IForm form)
        {
            var type = form.CallbackType;
            var result = new List<InlineKeyboardButton[]>
            {
                ([
                    InlineKeyboardButton.WithCallbackData("❌Отклонить",  $"{type}form-deny-{form.UserId}"),
                    InlineKeyboardButton.WithCallbackData("⚠️Сбросить",   $"{type}form-reset-{form.UserId}")
                ])
            };

            if (form is StudentForm)
            {
                result.Add([InlineKeyboardButton.WithCallbackData("✅Принять", $"{type}form-accept-{form.UserId}")]);
            }

            else
            {
                result.AddRange(
                    [
                        [ InlineKeyboardButton.WithCallbackData("📩Запросить ЛС", $"{type}form-requestPM-{form.UserId}") ],
                        [ InlineKeyboardButton.WithCallbackData("🥉Bronze",       $"{type}form-Bronze-{form.UserId}") ],
                        [ InlineKeyboardButton.WithCallbackData("🥈Silver",       $"{type}form-Silver-{form.UserId}") ],
                        [ InlineKeyboardButton.WithCallbackData("🥇Gold",         $"{type}form-Gold-{form.UserId}") ],
                        [ InlineKeyboardButton.WithCallbackData("💎Brilliant",    $"{type}form-Brilliant-{form.UserId}") ]
                    ]);
            }

            return new([.. result]);
        }

        public static InlineKeyboardMarkup ControlPanelMainMenu(RvUser rvUser) => new(
        [
            [ InlineKeyboardButton.WithCallbackData(Phrases.Lang[rvUser.Lang].ControlPanel.KeyboardButtons.ExploreUsers, $"control-{rvUser.UserId}-exploreUsers") ],
            [ InlineKeyboardButton.WithCallbackData(Phrases.Lang[rvUser.Lang].ControlPanel.KeyboardButtons.ExploreCritics, $"control-{rvUser.UserId}-exploreCritics") ],
            [ InlineKeyboardButton.WithCallbackData(Phrases.Lang[rvUser.Lang].ControlPanel.KeyboardButtons.ExploreParticipants, $"control-{rvUser.UserId}-exploreParticipants") ]
        ]);

        public static InlineKeyboardMarkup AcademyMainMenu(Role role, Lang lang, bool hasPermission)
        {
            var phrases = Phrases.Lang[lang].KeyboardButtons;
            var layers = new List<InlineKeyboardButton[]>();

            if (hasPermission)
            {
                var (buttonPhrase, callback) = role switch
                {
                    Role.Student => (phrases.StudentMenu, "studentMenu"),
                    Role.Teacher => (phrases.TeacherMenu, "teacherMenu"),
                    _ => (phrases.AcademyForm, "studentForm"),
                };
                layers.Add([InlineKeyboardButton.WithCallbackData(buttonPhrase, callback)]);
            }

            layers.Add([InlineBack(lang)]);

            return new InlineKeyboardMarkup([.. layers]);
        }

        #endregion

        #region Reply keyboards

        public static ReplyKeyboardMarkup ReplyBack(Lang lang) => new(new KeyboardButton(Phrases.Lang[lang].KeyboardButtons.Back)) { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup RateSelection(Lang lang) =>
            new(
            [
                [
                    new KeyboardButton("1"), new KeyboardButton("2"), new KeyboardButton("3"), new KeyboardButton("4")
                ],
                [
                    new KeyboardButton(Phrases.Lang[lang].KeyboardButtons.Back)
                ]
            ])
            { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup ReplyMainMenu => new(new KeyboardButton("/menu")) { ResizeKeyboard = true };

        #endregion

    }
}