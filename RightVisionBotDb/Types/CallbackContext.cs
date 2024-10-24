using RightVisionBotDb.Data;
using RightVisionBotDb.Models;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Types
{
    public class CallbackContext
    {
        #region Properties

        public RvUser RvUser { get; }
        public CallbackQuery CallbackQuery { get; }
        public ApplicationDbContext DbContext { get; }

        #endregion

        #region Constructor

        public CallbackContext(RvUser rvUser, CallbackQuery? callbackQuery, ApplicationDbContext dbContext)
        {
            RvUser = rvUser ?? throw new NullReferenceException(nameof(rvUser));
            CallbackQuery = callbackQuery ?? throw new NullReferenceException(nameof(callbackQuery));
            DbContext = dbContext ?? throw new NullReferenceException(nameof(dbContext));
        }

        #endregion
    }
}
