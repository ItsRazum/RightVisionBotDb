using DryIoc;
using RightVisionBotDb.Models;
using Telegram.Bot;

namespace RightVisionBotDb.Services
{
    public class RvLogger
    {
        public async void Log(string message, RvUser rvUser)
        {
            var bot = App.Container.Resolve<Bot>();
            await bot.Client.SendTextMessageAsync(-4074101060, message + $"\n=====\nId:{rvUser.UserId}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.Location}", disableNotification: true);
        }
    }
}
