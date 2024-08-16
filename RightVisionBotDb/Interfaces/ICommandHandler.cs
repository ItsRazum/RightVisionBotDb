using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Interfaces
{
    internal interface ICommandHandler
    {
        Task HandleCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
        Task HandleCallbackAsync(ITelegramBotClient client, CallbackQuery callbackQuery, CancellationToken cancellationToken);
    }
}
