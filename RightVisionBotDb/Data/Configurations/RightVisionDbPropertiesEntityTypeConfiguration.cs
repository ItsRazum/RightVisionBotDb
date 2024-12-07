using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Data.Configurations
{
    internal class RightVisionDbPropertiesEntityTypeConfiguration : IEntityTypeConfiguration<DbProperties>
    {
        public void Configure(EntityTypeBuilder<DbProperties> builder)
        {
            builder.Property(r => r.RightVisionStatus)
                .HasConversion<string>();

            builder.Property(r => r.EnrollmentStatus)
                .HasConversion<string>();
        }
    }
}
