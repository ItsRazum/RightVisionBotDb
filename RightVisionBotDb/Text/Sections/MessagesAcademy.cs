using RightVisionBotDb.Text.Interfaces;

namespace RightVisionBotDb.Text.Sections
{
    public class MessagesAcademy : IFormMessages
    {
        public string EnrollmentClosed { get; set; }
        public string FormIntro { get; set; }
        public string AreYouReady { get; set; }
        public string ClarifyProperties { get; set; }
        public string EnterName { get; set; }
        public string EnterLink { get; set; }
        public string EnterRate { get; set; }
        public string FormSubmitted { get; set; }
        public string FormAccepted { get; set; }
        public string FormDenied { get; set; }
        public string PMRequested { get; set; }
        public string FormCanceled { get; set; }
        public string FormBlocked { get; set; }
    }
}
