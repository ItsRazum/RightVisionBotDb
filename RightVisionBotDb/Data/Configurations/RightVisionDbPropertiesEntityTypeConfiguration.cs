using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Data.Configurations
{
    internal class RightVisionDbPropertiesEntityTypeConfiguration : IEntityTypeConfiguration<RightVisionDbProperties>
    {
        public void Configure(EntityTypeBuilder<RightVisionDbProperties> builder)
        {
            builder.Property(r => r.RightVisionStatus)
                .HasConversion<string>();

            builder.Property(r => r.EnrollmentStatus)
                .HasConversion<string>();
        }
    }
}
