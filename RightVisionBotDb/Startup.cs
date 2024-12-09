using DryIoc;
using RightVisionBotDb.Converters;
using RightVisionBotDb.Data.Configurations;
using RightVisionBotDb.Data.Contexts;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using Serilog;

namespace RightVisionBotDb
{
    /// <summary>
    /// InDev
    /// </summary>
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
            RegisterDatabase(container);

            container.Register<Bot>(Reuse.Singleton);

            //container.Resolve<Bot>().Configure();
        }

        private static void RegisterServices(IContainer container)
        {
            Log.Logger?.Information("Регистрация сервисов...");
            container.RegisterInstance(Log.Logger);
            container.Register<LocationService>(Reuse.Singleton);
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
            Log.Logger?.Information("Регистрация конфигураций сущностей...");
            container.Register<CriticFormEntityTypeConfiguration>(Reuse.Singleton);
            container.Register<ParticipantFormEntityTypeConfiguration>(Reuse.Singleton);
            container.Register<RightVisionDbPropertiesEntityTypeConfiguration>(Reuse.Singleton);
            container.Register<RvUserEntityTypeConfiguration>(Reuse.Singleton);

            container.Register<ApplicationDbContext>(Reuse.Scoped);
        }
    }
}