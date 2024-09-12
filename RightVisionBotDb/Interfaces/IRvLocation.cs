using RightVisionBotDb.Data;
using RightVisionBotDb.Models;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Interfaces
{
    public interface IRvLocation
    {
        public Task HandleCommandAsync(Message message, RvUser rvUser, ApplicationDbContext context, CancellationToken token);
        public Task HandleCallbackAsync(CallbackQuery callbackQuery, RvUser rvUser, ApplicationDbContext context, CancellationToken token);
    }
}
