using DryIoc;
using RightVisionBotDb.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Types
{
    public abstract class RvLocation
    {
        public required IReplyMarkup Markup { get; set; }
        public HashSet<RvUser> RvUsers { get; protected set; }


        public event EventHandler<RvUser>? UserAdded;
        public abstract void AddNewUser(RvUser rvUser);
        public abstract string Text(Enums.Lang lang);

        public void InvokeUserAdded(object? sender, RvUser rvUser)
        {
            UserAdded?.Invoke(sender, rvUser);
        }
    }
}
