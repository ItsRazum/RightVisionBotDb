using RightVisionBotDb.Services;
using RightVisionBotDb.Types;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    public class PublicChat : RvLocation
    {

        #region Constructor

        public PublicChat(
            Bot bot, 
            Keyboards keyboards, 
            LocationManager locationManager, 
            RvLogger logger, 
            LogMessages logMessages, 
            LocationsFront locationsFront) 
            : base(bot, keyboards, locationManager, logger, logMessages, locationsFront)
        {
            this
                .RegisterTextCommand("/ban", BanCommand);
        }

        #endregion

        #region Methods

        private async Task BanCommand(CommandContext c, CancellationToken token)
        {
            var commandArgs = c.Message.Text!.Trim().Split(' ');
            switch (commandArgs.Length)
            {
                case 1:
                    if (c.Message.ReplyToMessage == null)
                    {
                        await Bot.Client.SendTextMessageAsync(c.Message.Chat, "Данная команда используется только по отношению к другому пользователю!", cancellationToken: token);
                        return;
                    }
                    else
                    {
                        await Bot.Client.SendTextMessageAsync(c.Message.Chat, "");
                        break;
                    }
                case 2:
                    break;
                case 3:
                    break;
                default:
                    await Bot.Client.SendTextMessageAsync(c.Message.Chat, "Слишком много аргументов в команде!");
                    break;
            }
        }

        #endregion
    }
}
