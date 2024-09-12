using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Permissions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Services
{
    public sealed class Keyboards
    {
        public InlineKeyboardMarkup СhooseLang => new(new InlineKeyboardButton[][]
        {
            [
                InlineKeyboardButton.WithCallbackData("🇷🇺RU / CIS", Enums.Lang.Ru.ToString())
            ],
            [
                InlineKeyboardButton.WithCallbackData("🇺🇦UA", Enums.Lang.Ua.ToString()),
                InlineKeyboardButton.WithCallbackData("🇰🇿KZ", Enums.Lang.Kz.ToString())
            ]
        });

        public InlineKeyboardMarkup Hub(RvUser rvUser) => new(new[]
{
            [
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.About, "about"),
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.Apply, "forms")
            ],
            [
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.Academy, "academy")
            ],
            new[] {
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.MyProfile, "profile")
            }
        });

        public InlineKeyboardMarkup About(RvUser rvUser) => new(new[]
{
            InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.Back, "mainmenu"),
            InlineKeyboardButton.WithCallbackData("Информация про бота", "aboutBot")
        });

        public InlineKeyboardMarkup InlineBack(RvUser rvUser) =>
            new[]
            {
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.Back, "back"),
            };

        public static InlineKeyboardMarkup Profile(RvUser rvUser, ChatType type, Enums.Lang lang)
        {
            InlineKeyboardButton[] top =
            [
                InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.PermissionsList, $"permissions-{rvUser.UserId}"),
                InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.PunishmentsHistory, $"history-{rvUser.UserId}")
            ];

            InlineKeyboardButton[] back =
            [
                InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.Back, "mainmenu")
            ];

            InlineKeyboardButton[] criticMenu =
            [
                InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.CriticMenu.Open, "opencriticmenu")
            ];

            InlineKeyboardButton[] bottom =
            [
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.Apply, "forms")
            ];

            InlineKeyboardButton[] memberButtons =
            [
                //InlineKeyboardButton.WithCallbackData("✏️" + Language.GetPhrase("Keyboard_Choice_EditTrack", lang), "m_edittrack"),
                //InlineKeyboardButton.WithCallbackData("📇" + Language.GetPhrase("Keyboard_Choice_SendTrack", lang), "m_openmenu"),
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.GetVisual, "getvisual")
            ];

            InlineKeyboardButton[] memberOptions = memberButtons;
            InlineKeyboardButton[] criticOptions = criticMenu;
            InlineKeyboardButton[] customOptions = criticOptions;
            InlineKeyboardMarkup criticAndMember = new(new[] { top, back, criticOptions, memberOptions, bottom });

            if (rvUser.Has(Permission.TrackCard) && !rvUser.Has(Permission.CriticMenu))
                customOptions = memberOptions;

            InlineKeyboardMarkup custom = new(new[]
            {
                top, back, customOptions, bottom
            });

            InlineKeyboardMarkup common = new(new[] { top, back, bottom });

            if (!rvUser.Has(Permission.TrackCard) && !rvUser.Has(Permission.CriticMenu))
                custom = common;
            else if (rvUser.Has(Permission.TrackCard) && rvUser.Has(Permission.CriticMenu))
                custom = criticAndMember;
            return type != ChatType.Private ? top : custom;
        }
    }
}