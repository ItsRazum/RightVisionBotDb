﻿namespace RightVisionBotDb.Text.Sections
{
    public class ProfilePunishment
    {
        public string Ban { get; set; }
        public string Mute { get; set; }
        public string InParticipants { get; set; }
        public string InCritics { get; set; }
        public string InAcademyGeneralChat { get; set; }
        public string InAcademyClassChat { get; set; }
        public string Reason { get; set; }
        public string DateTo { get; set; }
        public string NoReason { get; set; }
        public string NoPunishments { get; set; }
        public PunishmentButtons Buttons { get; set; }
        public PunishmentTimeLeftFormat TimeLeftFormat { get; set; }
    }
}
