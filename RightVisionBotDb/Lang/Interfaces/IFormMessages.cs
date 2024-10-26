namespace RightVisionBotDb.Lang.Interfaces
{
    internal interface IFormMessages
    {
        public string PMRequested { get; set; }
        public string FormAccepted { get; set; }
        public string FormDenied { get; set; }
        public string FormCanceled { get; set; }
        public string FormBlocked { get; set; }
    }
}
