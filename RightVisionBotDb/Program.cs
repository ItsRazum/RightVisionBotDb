using DryIoc;
using RightVisionBotDb.Converters;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using Serilog;

namespace RightVisionBotDb
{
    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            #region Initialization

            SQLitePCL.Batteries.Init();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger?.Information("================= RightVisionBot =================");

            Log.Logger?.Information("Регистрация сервисов...");
            App.Container.RegisterInstance(Log.Logger);
            App.Container.Register<LocationService>(Reuse.Singleton);
            App.Container.Register<RvLogger>(Reuse.Singleton);
            App.Container.Register<LocationsFront>(Reuse.Singleton);
            App.Container.Register<CriticFormService>(Reuse.Singleton);
            App.Container.Register<ParticipantFormService>(Reuse.Singleton);
            App.Container.Register<StudentFormService>(Reuse.Singleton);
            App.Container.Register<LocationConverter>(Reuse.Singleton);
            App.Container.Register<UserPermissionsConverter>(Reuse.Singleton);
            App.Container.Register<ShellService>(Reuse.Singleton);
            App.Container.Register<TrackCardService>(Reuse.Singleton);

            App.Container.Register<Bot>(Reuse.Singleton);

            App.Container.Resolve<Bot>().Configure(args);

            #endregion
        }
    }
}
