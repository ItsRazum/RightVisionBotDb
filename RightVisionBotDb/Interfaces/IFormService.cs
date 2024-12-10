using RightVisionBotDb.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Interfaces
{
    public interface IFormService
    {
        #region Properties

        public Dictionary<int, Func<Lang, (string message, ReplyKeyboardMarkup? keyboard)>> Messages { get; }

        #endregion
    }
}
