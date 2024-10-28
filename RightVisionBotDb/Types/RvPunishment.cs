using RightVisionBotDb.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;

namespace RightVisionBotDb.Types
{
    public class RvPunishment
    {

        #region Fields

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RvUserId { get; private set; }
        public PunishmentType Type { get; set; }
        public long GroupId { get; set; }
        public string? Reason { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        #endregion

        #region Constructor

        public RvPunishment(PunishmentType type, long groupId, string? reason, DateTime from, DateTime to)
        {
            Type = type;
            GroupId = groupId;
            Reason = reason;
            From = from;
            To = to;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append($"{Type};");
            sb.Append(GroupId + ";");
            sb.Append(Reason + ";");
            sb.Append(From.ToString(CultureInfo.GetCultureInfo("en-US").DateTimeFormat) + ";");
            sb.Append(To.ToString(CultureInfo.GetCultureInfo("en-US").DateTimeFormat) + ",");
            return sb.ToString();
        }

        #endregion

    }
}
