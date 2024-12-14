using DryIoc.ImTools;
using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Models;
using RightVisionBotDb.Types;

namespace RightVisionBotDb.Helpers
{
    internal class CommandFormatHelper
    {
        public static async Task<(RvUser? extractedRvUser, string[] args)> ExtractRvUserFromArgs(CommandContext c, CancellationToken token = default)
        {
            var args = c.Message.Text!
                .Trim()
                .Split(' ');
            var replied = c.Message.ReplyToMessage != null;

            var userTag = replied
                ? c.Message.ReplyToMessage!.From!.Id.ToString()
                : args.ElementAtOrDefault(1);

            RvUser? rvUser = null;

            var filteredRvUsers = await c.DbContext.RvUsers
                .Where(u => u.Lang != Lang.Na)
                .ToListAsync(token);

            if (userTag != null)
            {
                if (userTag == "@x")
                    return (null, args);

                rvUser = long.TryParse(userTag, out var userId)
                    ? filteredRvUsers.FirstOrDefault(u => u.UserId == userId)
                    : filteredRvUsers.FirstOrDefault(u => "@" + u.Telegram == userTag);
            }
            args = args.RemoveAt(0);

            if (!replied && args.Length > 0)
                args = args.RemoveAt(0);

            return (rvUser, args);
        }
    }
}
