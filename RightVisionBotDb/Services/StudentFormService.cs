using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Services
{
    public class StudentFormService : IFormService
    {
        public Dictionary<int, Func<Lang, (string message, ReplyKeyboardMarkup? keyboard)>> Messages { get; }

        public StudentFormService() 
        {

        }
    }
}
