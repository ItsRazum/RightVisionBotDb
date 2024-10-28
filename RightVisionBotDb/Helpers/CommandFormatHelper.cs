using DryIoc.ImTools;
using RightVisionBotDb.Models;
using RightVisionBotDb.Types;

namespace RightVisionBotDb.Helpers
{
    internal class CommandFormatHelper
    {
        public static (RvUser? extractedRvUser, string[] args) ExtractRvUserFromArgs(CommandContext c)
        {
            var args = c.Message.Text!
                .Trim()
                .Split(' ');
            var replied = c.Message.ReplyToMessage != null;

            var userTag = replied
                ? c.Message.ReplyToMessage!.From!.Id.ToString()
                : args.ElementAtOrDefault(1);

            RvUser? rvUser = null;

            if (userTag != null)
            {
                rvUser = long.TryParse(userTag, out var userId)
                    ? c.DbContext.RvUsers.FirstOrDefault(u => u.UserId == userId)
                    : c.DbContext.RvUsers.FirstOrDefault(u => "@" + u.Telegram == userTag);
            }

            args = args.RemoveAt(0);

            if (!replied && args.Length > 0)
                args = args.RemoveAt(0);

            return (rvUser, args);
        }
    }
}
