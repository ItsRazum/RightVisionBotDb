using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Types
{
    public class CommandResult
    {
        public required Chat TargetChat { get; set; }
        public string? Message { get; set; }
        public string? LogMessage { get; set; }
        public ReplyKeyboardMarkup? ReplyKeyboardMarkup { get; set; }
    }
}
