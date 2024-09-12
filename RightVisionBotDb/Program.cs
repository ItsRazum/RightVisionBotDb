using DryIoc;
using RightVisionBotDb.Services;
using Serilog;

namespace RightVisionBotDb
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            SQLitePCL.Batteries.Init();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger?.Information("================= RightVisionBot =================");

            Log.Logger?.Information("Регистрация сервисов...");
            App.Container.RegisterInstance(Log.Logger);
            App.Container.Register<DatabaseService>(Reuse.Singleton);
            App.Container.Register<LocationManager>(Reuse.Singleton);
            App.Container.Register<RvLogger>(Reuse.Singleton);
            App.Container.Register<ProfileStringService>(Reuse.Singleton);
            App.Container.Register<LogMessages>(Reuse.Singleton);
            App.Container.Register<Keyboards>(Reuse.Singleton);

            App.Container.Register<Bot>(Reuse.Singleton);

            App.Container.Resolve<Bot>().RegisterBot();

            Console.ReadLine();
        }
    }
}
