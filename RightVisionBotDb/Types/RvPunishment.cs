using RightVisionBotDb.Enums;

namespace RightVisionBotDb.Types
{
    public class RvPunishment
    {

        #region Fields

        public PunishmentType Type { get; set; }
        public long GroupId { get; set; }
        public long ModeratorId { get; set; }
        public string? Reason { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        #endregion

        #region Constructor

        public RvPunishment(PunishmentType type, long groupId, long moderatorId, string? reason, DateTime startDateTime, DateTime endDateTime)
        {
            Type = type;
            GroupId = groupId;
            ModeratorId = moderatorId;
            Reason = reason;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }

        #endregion

    }
}
