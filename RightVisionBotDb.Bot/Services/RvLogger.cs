using RightVisionBotDb.Models;
using Telegram.Bot;

namespace RightVisionBotDb.Bot.Services
{
    public class RvLogger
    {
        public async void Log(string message, RvUser rvUser, Bot bot)
        {
            await bot.Client.SendTextMessageAsync(-4074101060, message + $"\n=====\nId:{rvUser.UserId}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.Location}", disableNotification: true);
        }
    }
}
