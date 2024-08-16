using RightVisionBotDb.Models;
using Telegram.Bot.Types.ReplyMarkups;
using RightVisionBotDb.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Types
{
    public abstract class RvLocation : ICommandHandler
    {
        public required IReplyMarkup Markup { get; set; }
        public HashSet<RvUser> RvUsers { get; protected set; } = new HashSet<RvUser>();


        public event EventHandler<RvUser>? UserAdded;

        public abstract Task HandleCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
        public abstract Task HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken);
        public abstract void AddNewUser(RvUser rvUser);
        public abstract string Text(Enums.Lang lang);

        public void InvokeUserAdded(object? sender, RvUser rvUser)
        {
            UserAdded?.Invoke(sender, rvUser);
        }
    }
}
