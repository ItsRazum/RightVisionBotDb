using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Data.Configurations
{
    public class StudentFormEntityTypeConfiguration : IEntityTypeConfiguration<StudentForm>
    {
        public void Configure(EntityTypeBuilder<StudentForm> builder)
        {
            //builder
            //    .HasOne(s => s.Teacher)
            //    .WithMany(t => t.Students)
            //    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
