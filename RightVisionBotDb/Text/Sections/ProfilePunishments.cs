﻿namespace RightVisionBotDb.Text.Sections
{
    public class ProfilePunishments
    {
        public string Contacts { get; set; }
        public string Notification { get; set; }
        public string UnbanNotification { get; set; }
        public string UnmuteNotification { get; set; }
        public string BlacklistNotification { get; set; }
        public string BlacklistOffNotificatiom { get; set; }
        public string HareKickNotification { get; set; }
        public ProfilePunishment Punishment { get; set; }
    }
}