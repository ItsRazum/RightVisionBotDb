using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Data.Configurations
{
    internal class ParticipantFormEntityTypeConfiguration : IEntityTypeConfiguration<ParticipantForm>
    {
        public void Configure(EntityTypeBuilder<ParticipantForm> builder)
        {
            builder.Property(p => p.Category)
                .HasConversion<string>();

            builder.Property(p => p.Status)
                .HasConversion<string>();
        }
    }
}

