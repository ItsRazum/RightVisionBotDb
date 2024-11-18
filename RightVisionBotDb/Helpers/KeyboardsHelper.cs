﻿using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Lang.Phrases;
using RightVisionBotDb.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Helpers
{
    public static class KeyboardsHelper
    {

        #region Inline keyboards

        public static InlineKeyboardMarkup СhooseLang = new(
        [
            [
                InlineKeyboardButton.WithCallbackData("🇷🇺RU / CIS", Enums.Lang.Ru.ToString())
            ],
            [
                InlineKeyboardButton.WithCallbackData("🇺🇦UA", Enums.Lang.Ua.ToString()),
                InlineKeyboardButton.WithCallbackData("🇰🇿KZ", Enums.Lang.Kz.ToString())
            ]
        ]);

        public static InlineKeyboardMarkup MainMenu(RvUser rvUser) => new(
        [
            [
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.About, "about"),
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.Apply, "forms")
            ],
            [
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.Academy, "academy")
            ],
            [
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.MyProfile, $"profile-{rvUser.UserId}")
            ]
        ]);

        public static InlineKeyboardMarkup About(Enums.Lang lang) => new(
[
            InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.Back, "back"),
            InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.AboutBot, "aboutBot")
        ]);

        public static InlineKeyboardMarkup InlineBack(Enums.Lang lang) =>
            new[]
            {
                InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.Back, "back")
            };

        public static InlineKeyboardMarkup BackToAbout(Enums.Lang lang) =>
            new[]
            {
                InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.Back, "about")
            };

        public static async Task<InlineKeyboardMarkup> Profile(RvUser rvUser, ChatType type, string rightvision, Enums.Lang lang)
        {
            var userId = rvUser.UserId;
            var rightVisions = App.AllRightVisions;
            var isOldParticipant = false;
            string? firstParticipationRightVision = null;
            var phrases = Language.Phrases[lang];

            foreach (var rightvisionName in rightVisions)
            {
                using var rvdb = DatabaseHelper.GetRightVisionContext(rightvisionName);
                var form = await rvdb.ParticipantForms
                    .FirstOrDefaultAsync(
                        p => p.UserId == userId && p.Status == FormStatus.Accepted);

                if (form != null && rightvisionName != App.DefaultRightVision)
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


            if (rvUser.Has(Permission.TrackCard))
                keyboardLayers.Add([
				//InlineKeyboardButton.WithCallbackData("✏️" + Language.GetPhrase("Keyboard_Choice_EditTrack", lang), "m_edittrack"),
				//InlineKeyboardButton.WithCallbackData("📇" + Language.GetPhrase("Keyboard_Choice_SendTrack", lang), "m_openmenu"),
				InlineKeyboardButton.WithCallbackData(phrases.KeyboardButtons.GetVisual, "getvisual")
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

            var participantButton = InlineKeyboardButton.WithCallbackData(
                Language.Phrases[rvUser.Lang].KeyboardButtons.ParticipantFormVariationOne,
                "participantForm"
            );
            var criticButton = InlineKeyboardButton.WithCallbackData(
                Language.Phrases[rvUser.Lang].KeyboardButtons.CriticFormVariationOne,
                "criticForm"
            );
            var backButton = InlineKeyboardButton.WithCallbackData(
                Language.Phrases[rvUser.Lang].KeyboardButtons.Back,
                "back"
            );

            var upperFloor = new List<InlineKeyboardButton>();

            if (rvUser.Has(Permission.SendParticipantForm))
                upperFloor.Add(participantButton);

            if (rvUser.Has(Permission.SendCriticForm))
                upperFloor.Add(criticButton);

            buttons.Add([.. upperFloor]);
            buttons.Add([backButton]);

            return new InlineKeyboardMarkup(buttons);
        }

        public static InlineKeyboardMarkup UserParticipations(RvUser rvUser, Enums.Lang lang)
        {
            return InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.Back, $"profile-{rvUser.UserId}");
        }

        public static InlineKeyboardMarkup PermissionsList(RvUser rvUser, bool minimize, bool showAdvancedOptions, Enums.Lang lang)
        {
            List<InlineKeyboardButton> buttons = [];
            if (showAdvancedOptions)
            {
                var buttonValues = minimize
                ? (Language.Phrases[lang].KeyboardButtons.Maximize, $"permissions_maximized-{rvUser.UserId}")
                : (Language.Phrases[lang].KeyboardButtons.Minimize, $"permissions_minimized-{rvUser.UserId}");

                buttons.Add(InlineKeyboardButton.WithCallbackData(buttonValues.Item1, buttonValues.Item2));
            }

            buttons.Add(InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.Back, $"profile-{rvUser.UserId}"));

            return new(buttons);
        }

        public static InlineKeyboardMarkup PunishmentsList(RvUser rvUser, bool bansCheckBoxEnabled, bool mutesCheckBoxEnabled, Enums.Lang lang)
        {
            var backButton = InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.Back, $"profile-{rvUser.UserId}");

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
                    banCheckBox + " " + Language.Phrases[rvUser.Lang].Profile.Punishments.Punishment.Buttons.ShowBans,
                    banCallback)]
                    );

                var (muteCheckBox, muteCallback) = getButtonParts(mutesCheckBoxEnabled, PunishmentType.Mute, rvUser.UserId);
                layers.Add([InlineKeyboardButton.WithCallbackData(
                    muteCheckBox + " " + Language.Phrases[rvUser.Lang].Profile.Punishments.Punishment.Buttons.ShowMutes,
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
            return new(new InlineKeyboardButton[][]
            {
                [
                    InlineKeyboardButton.WithCallbackData("❌Отклонить",  $"{type}form-deny-{form.UserId}"),
                    InlineKeyboardButton.WithCallbackData("⚠️Сбросить",   $"{type}form-reset-{form.UserId}")
                ],
                [ InlineKeyboardButton.WithCallbackData("📩Запросить ЛС", $"{type}form-requestPM-{form.UserId}") ],
                [ InlineKeyboardButton.WithCallbackData("🥉Bronze",       $"{type}form-Bronze-{form.UserId}") ],
                [ InlineKeyboardButton.WithCallbackData("🥈Silver",       $"{type}form-Silver-{form.UserId}") ],
                [ InlineKeyboardButton.WithCallbackData("🥇Gold",         $"{type}form-Gold-{form.UserId}") ],
                [ InlineKeyboardButton.WithCallbackData("💎Brilliant",    $"{type}form-Brilliant-{form.UserId}") ]
            });
        }

        public static InlineKeyboardMarkup ControlPanelMainMenu(RvUser rvUser) => new(new InlineKeyboardButton[][]
        {
            [ InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].ControlPanel.KeyboardButtons.ExploreUsers, $"control-{rvUser.UserId}-exploreUsers") ],
            [ InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].ControlPanel.KeyboardButtons.ExploreCritics, $"control-{rvUser.UserId}-exploreCritics") ],
            [ InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].ControlPanel.KeyboardButtons.ExploreParticipants, $"control-{rvUser.UserId}-exploreParticipants") ]
        });

        #endregion

        #region Reply keyboards

        public static ReplyKeyboardMarkup ReplyBack(Enums.Lang lang) => new(new KeyboardButton(Language.Phrases[lang].KeyboardButtons.Back)) { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup RateSelection(Enums.Lang lang) =>
            new(new KeyboardButton[][]
            {
                [
                    new KeyboardButton("1"), new KeyboardButton("2"), new KeyboardButton("3"), new KeyboardButton("4")
                ],
                [
                    new KeyboardButton(Language.Phrases[lang].KeyboardButtons.Back)
                ]
            })
            { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup ReplyMainMenu => new(new KeyboardButton("/menu")) { ResizeKeyboard = true };

        #endregion

    }
}