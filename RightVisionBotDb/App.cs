using DryIoc;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Settings;

namespace RightVisionBotDb
{
    public static class App
    {
        public static Configuration Configuration { get; set; }
        public static IContainer Container { get; set; } = new Container();
        public static Lang[] RegisteredLangs { get; set; }
        public static string[] AllAcademies { get; set; }
        public static string[] AllRightVisions { get; set; }
    }
}
