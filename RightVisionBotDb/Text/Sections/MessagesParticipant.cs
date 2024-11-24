using RightVisionBotDb.Text.Interfaces;

namespace RightVisionBotDb.Text.Sections
{
    public class MessagesParticipant : IFormMessages
    {
        public string EnterName { get; set; }
        public string EnterLink { get; set; }
        public string EnterRate { get; set; }
        public string EnterTrack { get; set; }
        public string FormSubmitted { get; set; }
        public string PMRequested { get; set; }
        public string FormAccepted { get; set; }
        public string FormDenied { get; set; }
        public string FormCanceled { get; set; }
        public string FormBlocked { get; set; }
        public string PreListeningBlocked { get; set; }
        public string PreListeningCategoryChanged { get; set; }
        public string EnterNewTrack { get; set; }
        public string TrackUpdated { get; set; }
        public ParticipantTrackCard TrackCard { get; set; }
    }
}
