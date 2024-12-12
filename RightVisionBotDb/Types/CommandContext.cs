using RightVisionBotDb.Data.Contexts;
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
        public IApplicationDbContext DbContext { get; }
        public IRightVisionDbContext RvContext { get; }
        public IAcademyDbContext AcademyContext { get; }

        #endregion

        #region Constructor

        public CommandContext(RvUser rvUser, Message message, IApplicationDbContext dbContext, IRightVisionDbContext rightVisionDbContext, IAcademyDbContext academyDbContext)
        {
            RvUser = rvUser ?? throw new NullReferenceException(nameof(rvUser));
            Message = message ?? throw new NullReferenceException(nameof(message));
            DbContext = dbContext ?? throw new NullReferenceException(nameof(dbContext));
            RvContext = rightVisionDbContext ?? throw new NullReferenceException(nameof(rightVisionDbContext));
            AcademyContext = academyDbContext ?? throw new NullReferenceException(nameof(academyDbContext));
        }

        #endregion
    }
}
