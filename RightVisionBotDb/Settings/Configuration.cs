using Microsoft.Extensions.Configuration;

namespace RightVisionBotDb.Settings
{
    public class Configuration
    {
        public Configuration(IConfiguration configuration) 
        {
            configuration.Bind(this);
        }

        public BotSettings BotSettings { get; set; } = new();
        public DataSettings DataSettings { get; set; } = new();
        public ContestSettings ContestSettings { get; set; } = new();
        public UISettings UISettings { get; set; } = new();
    }
}
