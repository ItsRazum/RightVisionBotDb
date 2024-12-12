using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Interfaces
{
    public interface IMultipleDbContext
    {
        string Name { get; }
        string DatabasesDir { get; }
        RightVisionStatus Status
        {
            get => Properties.First().RightVisionStatus;
            set => Properties.First().RightVisionStatus = value;
        }

        EnrollmentStatus EnrollmentStatus
        {
            get => Properties.First().EnrollmentStatus;
            set => Properties.First().EnrollmentStatus = value;
        }
        DateOnly StartDate
        {
            get => Properties.First().StartDate;
            set => Properties.First().StartDate = value;
        }

        DateOnly? EndDate
        {
            get => Properties.First().EndDate;
            set => Properties.First().EndDate = value;
        }
        DbSet<DbProperties> Properties { get; }
    }
}
