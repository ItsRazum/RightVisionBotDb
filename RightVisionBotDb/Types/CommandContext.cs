using RightVisionBotDb.Data;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Types
{
    public class CommandContext : IContext
    {
        #region Properties

        public RvUser RvUser { get; }
        public Message Message { get; }
        public ApplicationDbContext DbContext { get; }
        public RightVisionDbContext RvContext { get; }

        #endregion

        #region Constructor

        public CommandContext(RvUser rvUser, Message message, ApplicationDbContext dbContext, RightVisionDbContext rightVisionDbContext)
        {
            RvUser = rvUser ?? throw new NullReferenceException(nameof(rvUser));
            Message = message ?? throw new NullReferenceException(nameof(message));
            DbContext = dbContext ?? throw new NullReferenceException(nameof(dbContext));
            RvContext = rightVisionDbContext ?? throw new NullReferenceException(nameof(rightVisionDbContext));
        }

        #endregion
    }
}
