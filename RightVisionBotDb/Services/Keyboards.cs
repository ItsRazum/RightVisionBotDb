using RightVisionBotDb.Data;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Models;
using RightVisionBotDb.Permissions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Services
{
    public sealed class Keyboards
    {
        private DatabaseService DatabaseService { get; }
        private LocationManager LocationManager { get; }

        public Keyboards(
            LocationManager locationManager,
            DatabaseService databaseService) 
        {
            LocationManager = locationManager;
            DatabaseService = databaseService;
        }

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

        public InlineKeyboardMarkup MainMenu(RvUser rvUser)
        {
            var rootLocation = LocationManager.LocationToString(LocationManager[nameof(Locations.MainMenu)]);
            return new(new[]
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
        }

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

        public async Task<InlineKeyboardMarkup> Profile(RvUser rvUser, ChatType type, string rightvision, Enums.Lang lang)
        {
            var userId = rvUser.UserId;
            var rightVisions = App.AllRightVisions;
            var participations = new Dictionary<string, ParticipantForm>();

            foreach (var rightvisionName in rightVisions)
            {
                using var rvdb = DatabaseService.GetRightVisionContext(rightvisionName);
                var form = rvdb.ParticipantForms
                .FirstOrDefault(
                        p => p.UserId == userId
                        && p.Status == Enums.FormStatus.Accepted
                        );

                if (form != null)
                    participations.Add(rightvisionName, form);
            }

            List<InlineKeyboardButton[]> keyboardLayers = new()
            {
                new [] { InlineKeyboardButton.WithCallbackData(rightvision, "global_rvProperties") }
            };

            List<InlineKeyboardButton> keyboardButtons = new();

            var keys = participations.Keys.ToList();
            var index = keys.IndexOf(rightvision);

            if (index == -1)
                throw new ArgumentException("Ключ не найден");

            var startIndex = Math.Max(0, index - 1);
            var endIndex = Math.Min(keys.Count - 1, index + 1);

            var result = new Dictionary<string, ParticipantForm>();

            for (var i = startIndex; i < endIndex; i++)
            {
                var currentKey = keys[i];
                result[currentKey] = participations[currentKey];
            }

            switch (result.Count)
            {
                case 2:
                    (string, string) values = result.Last().Key == rightvision
                        ? ($"« {result.First().Key}", $"profile-{userId}-{result.First().Key}")
                        : ($"{result.Last().Key} »", $"profile-{userId}-{result.Last().Key}");

                    keyboardButtons.Add(InlineKeyboardButton.WithCallbackData(values.Item1, values.Item2));
                    break;
                case 3:
                    keyboardButtons.Add(InlineKeyboardButton.WithCallbackData($"« {result.First().Key}", $"profile-{userId}-{result.First().Key}"));
                    keyboardButtons.Add(InlineKeyboardButton.WithCallbackData($"{result.Last().Key} »", $"profile-{userId}-{result.Last().Key}"));
                    break;
            }

            if (keyboardButtons.Count > 0)
                keyboardLayers.Add([.. keyboardButtons]);

            keyboardLayers.Add(
                [
                InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.PermissionsList, $"permissions_minimized-{rvUser.UserId}"),
                InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.PunishmentsHistory, $"history-{rvUser.UserId}")
            ]);

            if (type == ChatType.Private) return keyboardLayers.ToArray();

            keyboardLayers.Add([
                InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.Back, "mainmenu")
            ]);


            if (rvUser.Has(Permission.TrackCard))
                keyboardLayers.Add([
                //InlineKeyboardButton.WithCallbackData("✏️" + Language.GetPhrase("Keyboard_Choice_EditTrack", lang), "m_edittrack"),
                //InlineKeyboardButton.WithCallbackData("📇" + Language.GetPhrase("Keyboard_Choice_SendTrack", lang), "m_openmenu"),
                InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.GetVisual, "getvisual")
            ]);

            if (rvUser.Has(Permission.CriticMenu))
                keyboardLayers.Add([
                InlineKeyboardButton.WithCallbackData(Language.Phrases[lang].KeyboardButtons.CriticMenu.Open, "opencriticmenu")
            ]);

            return keyboardLayers.ToArray();
        }

        public InlineKeyboardMarkup FormSelection(RvUser rvUser)
        {
            var buttons = new List<InlineKeyboardButton[]>();

            var participantButton = InlineKeyboardButton.WithCallbackData(
                Language.Phrases[rvUser.Lang].KeyboardButtons.MemberFormVariationOne,
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

        public static InlineKeyboardMarkup PermissionsList(RvUser rvUser, bool minimize, bool showAdvancedOptions, Enums.Lang lang)
        {
            List<InlineKeyboardButton> buttons = new();
            if (showAdvancedOptions)
            {
                var buttonValues = (Language.Phrases[lang].KeyboardButtons.Minimize + "", $"permissions_minimized-{rvUser.UserId}");

                if (minimize)
                    buttonValues = (Language.Phrases[lang].KeyboardButtons.Maximize, $"permissions_maximized-{rvUser.UserId}");

                buttons.Add(InlineKeyboardButton.WithCallbackData(buttonValues.Item1, buttonValues.Item2));
            }

            buttons.Add(InlineKeyboardButton.WithCallbackData(Language.Phrases[rvUser.Lang].KeyboardButtons.Back, $"permissions_back-{rvUser.UserId}"));

            return new(buttons);
        }

        public ReplyKeyboardMarkup ReplyBack(Enums.Lang lang) => new(new KeyboardButton(Language.Phrases[lang].KeyboardButtons.Back)) { ResizeKeyboard = true };

        public InlineKeyboardMarkup CriticCuratorship(long userId) => new(InlineKeyboardButton.WithCallbackData("Взять кураторство над судьёй", $"c_accept-{userId}"));
    }
}