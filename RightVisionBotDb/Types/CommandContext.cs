using RightVisionBotDb.Data;
using RightVisionBotDb.Models;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Types
{
    public class CommandContext
    {
        #region Properties

        public RvUser RvUser { get; }
        public Message Message { get; }
        public ApplicationDbContext DbContext { get; }

        #endregion

        #region Constructor

        public CommandContext(RvUser rvUser, Message message, ApplicationDbContext dbContext)
        {
            RvUser = rvUser ?? throw new NullReferenceException(nameof(rvUser));
            Message = message ?? throw new NullReferenceException(nameof(message));
            DbContext = dbContext ?? throw new NullReferenceException(nameof(dbContext));
        }

        #endregion
    }
}
