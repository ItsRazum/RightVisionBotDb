using RightVisionBotDb.Lang.Interfaces;

namespace RightVisionBotDb.Lang.Phrases
{
    public class DaysFormat : ITimeFormat
    {
        public string Singular { get; set; }
        public string Plural { get; set; }
        public string Genitive { get; set; }
    }
}
