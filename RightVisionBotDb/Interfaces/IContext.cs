﻿using RightVisionBotDb.Data.Contexts;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Interfaces
{
    public interface IContext
    {
        RvUser RvUser { get; }
        ApplicationDbContext DbContext { get; }
        RightVisionDbContext RvContext { get; }
    }
}
