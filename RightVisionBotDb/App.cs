using DryIoc;
using Microsoft.Extensions.Configuration;

namespace RightVisionBotDb
{
    public static class App
    {
        public static IConfiguration Configuration;
        public static IContainer Container { get; set; } = new Container();
        public static string DefaultRightVision { get; set; }
    }
}
