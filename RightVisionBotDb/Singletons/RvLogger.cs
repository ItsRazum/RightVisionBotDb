using RightVisionBotDb.Models;
using Telegram.Bot;

namespace RightVisionBotDb.Singletons
{
    public class RvLogger
    {
        private Bot Bot { get; set; }

        public RvLogger(Bot bot)
        {
            Bot = bot;
        }

        public async Task Log(string message, RvUser rvUser, CancellationToken token = default)
        {
            await Bot.Client.SendTextMessageAsync(-4074101060, message + $"\n=====\nId:{rvUser.UserId}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.Location}", disableNotification: true);
        }
    }
}
