using RightVisionBotDb.Data.Contexts;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Interfaces
{
    public interface IContext
    {
        RvUser RvUser { get; }
        IApplicationDbContext DbContext { get; }
        IRightVisionDbContext RvContext { get; }
        IAcademyDbContext AcademyContext { get; }
    }
}
