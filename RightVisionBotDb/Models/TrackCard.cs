using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RightVisionBotDb.Models
{
    public class TrackCard
    {
        public string? TrackFileId { get; set; }
        public string? TextFileId { get; set; }
        public string? ImageFileId { get; set; }
        public string? VisualFileId { get; set; }
    }
}
