using RightVisionBotDb.Enums;

namespace RightVisionBotDb.Models
{
    public class DbProperties
    {
        public int Id { get; set; }
        public RightVisionStatus RightVisionStatus { get; set; } = RightVisionStatus.Relevant;
        public EnrollmentStatus EnrollmentStatus { get; set; } = EnrollmentStatus.Closed;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public DbProperties()
        {
            StartDate = DateOnly.FromDateTime(DateTime.Now);
        }
    }
}
