/*
using DryIoc;
using RightVisionBotDb.Converters;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using Serilog;

namespace RightVisionBotDb
{
    public class Startup
    {
        public static void Run(IContainer container)
        {
            SQLitePCL.Batteries.Init();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger?.Information("================= RightVisionBot =================");

            RegisterServices(container);

            container.Register<Bot>(Reuse.Singleton);

            container.Resolve<Bot>().Configure();
        }

        private static void RegisterServices(IContainer container)
        {
            Log.Logger?.Information("Регистрация сервисов...");
            container.RegisterInstance(Log.Logger);
            container.Register<LocationManager>(Reuse.Singleton);
            container.Register<RvLogger>(Reuse.Singleton);
            container.Register<LocationsFront>(Reuse.Singleton);
            container.Register<CriticFormService>(Reuse.Singleton);
            container.Register<ParticipantFormService>(Reuse.Singleton);
            container.Register<LocationConverter>(Reuse.Singleton);
            container.Register<UserPermissionsConverter>(Reuse.Singleton);
            container.Register<ShellService>(Reuse.Singleton);
        }

        private static void RegisterDatabase(IContainer container)
        {

        }
    }
}
*/