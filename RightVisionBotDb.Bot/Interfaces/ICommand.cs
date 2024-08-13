using Telegram.Bot.Types;

namespace RightVisionBotDb.Bot.Interfaces
{
    internal interface ICommand
    {
        public void OnCommandGet(object sender, Update update);
    }
}
