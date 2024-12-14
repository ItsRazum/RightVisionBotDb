using Microsoft.Extensions.Configuration;

namespace RightVisionBotDb.Settings
{
    public class Configuration
    {
        public Configuration(IConfiguration configuration)
        {
            configuration.Bind(this);
        }
        public string HiddenToken { get; set; }
        public BotSettings BotSettings { get; set; } = new();
        public AcademySettings AcademySettings { get; set; } = new();
        public RightVisionSettings RightVisionSettings { get; set; } = new();
        public UISettings UISettings { get; set; } = new();
    }
}
