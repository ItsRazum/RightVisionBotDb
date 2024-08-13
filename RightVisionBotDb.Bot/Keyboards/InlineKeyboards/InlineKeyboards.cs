using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Bot.Keyboards.InlineKeyboards
{
    internal sealed class InlineKeyboards
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


    }
}