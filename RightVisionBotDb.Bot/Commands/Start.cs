using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Bot.Commands
{
    internal class Start : CommandResult
    {
        public async Task Result(Bot bot) 
        {
            if (Message != null)
                await bot.Client.SendTextMessageAsync(TargetChat, Message, replyMarkup: ReplyKeyboardMarkup);
        }
    }
}
