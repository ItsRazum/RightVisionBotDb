﻿using RightVisionBotDb.Data.Contexts;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Types
{
    public class CallbackContext : IContext
    {
        #region Properties

        public RvUser RvUser { get; }
        public CallbackQuery CallbackQuery { get; }
        public ApplicationDbContext DbContext { get; }
        public RightVisionDbContext RvContext { get; }

        #endregion

        #region Constructor

        public CallbackContext(RvUser rvUser, CallbackQuery? callbackQuery, ApplicationDbContext dbContext, RightVisionDbContext rightVisionDbContext)
        {
            RvUser = rvUser ?? throw new NullReferenceException(nameof(rvUser));
            CallbackQuery = callbackQuery ?? throw new NullReferenceException(nameof(callbackQuery));
            DbContext = dbContext ?? throw new NullReferenceException(nameof(dbContext));
            RvContext = rightVisionDbContext ?? throw new NullReferenceException(nameof(rightVisionDbContext));
        }

        #endregion
    }
}
