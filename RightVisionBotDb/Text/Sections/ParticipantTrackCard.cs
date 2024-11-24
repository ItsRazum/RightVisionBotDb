namespace RightVisionBotDb.Text.Sections
{
    public class ParticipantTrackCard
    {
        public TrackCardProperties Properties { get; set; }
        public string CreatingCard { get; set; }
        public string CreatingCardSuccess { get; set; }
        public string HereItIs { get; set; }
        public string CardFull { get; set; }
        public string SendTrack { get; set; }
        public string TrackNotSent { get; set; }
        public string TrackHereItIs { get; set; }
        public string SendTrackInstruction { get; set; }
        public string SendTrackSuccess { get; set; }
        public string SendImage { get; set; }
        public string ImageNotSent { get; set; }
        public string ImageHereItIs { get; set; }
        public string SendImageInstruction { get; set; }
        public string SendImageSuccess { get; set; }
        public string SendText { get; set; }
        public string TextNotSent { get; set; }
        public string TextHereItIs { get; set; }
        public string SendTextInstruction { get; set; }
        public string SendTextSuccess { get; set; }

        public string CheckTrack { get; set; }
        public string CheckImage { get; set; }
        public string CheckText { get; set; }

        public string TrackReceived { get; set; }
        public string TrackNotReceived { get; set; }

        public string ImageReceived { get; set; }
        public string ImageNotReceived { get; set; }
    }
}
