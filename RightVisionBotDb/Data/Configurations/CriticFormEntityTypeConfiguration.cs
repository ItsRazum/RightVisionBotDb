using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Data.Configurations
{
    internal class CriticFormEntityTypeConfiguration : IEntityTypeConfiguration<CriticForm>
    {
        public void Configure(EntityTypeBuilder<CriticForm> builder)
        {
            builder.Property(c => c.Category)
                .HasConversion<string>();

            builder.Property(c => c.Status)
                .HasConversion<string>();
        }
    }
}
