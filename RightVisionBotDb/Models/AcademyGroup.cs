using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RightVisionBotDb.Models
{
    public class AcademyGroup
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public long TeacherId { get; set; }

        public RvTeacher Teacher { get; set; }

        public List<StudentForm> Students { get; set; }
    }
}
