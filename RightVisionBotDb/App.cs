using DryIoc;
using RightVisionBotDb.Settings;

namespace RightVisionBotDb
{
    public static class App
    {
        public static Configuration Configuration { get; set; }
        public static IContainer Container { get; set; } = new Container();
        public static string DefaultRightVision { get; set; }
        public static Enums.Lang[] RegisteredLangs { get; set; }
        public static string[] AllRightVisions { get; set; }
        public static string RightVisionDatabasesPath { get; set; }
    }
}
