using DryIoc;
using RightVisionBotDb.Singletons;
using Serilog;

namespace RightVisionBotDb
{
    internal sealed class Program
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
            App.Container.Register<LocationManager>(Reuse.Singleton);
            App.Container.Register<RvLogger>(Reuse.Singleton);
            App.Container.Register<LocationsFront>(Reuse.Singleton);
            App.Container.Register<CriticFormService>(Reuse.Singleton);

            App.Container.Register<Bot>(Reuse.Singleton);

            App.Container.Resolve<Bot>().Configure();

            Console.ReadLine();
        }
    }
}
