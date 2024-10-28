namespace RightVisionBotDb.Types
{
    public class Reward
    {
        public Reward(string icon, string description)
        {
            Icon = icon;
            Description = description;
        }

        public string Icon { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"{Icon}: {Description}";
        }
    }
}
