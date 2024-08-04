using DryIoc;
using RightVisionBotDb.Data;
using Serilog;

namespace RightVisionBotDb
{
    static class Startup
    {
        public static void Run()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Logger?.Information("================= RightVisionBot =================");
            
            App.Container.RegisterInstance(Log.Logger);

            App.Container.Register<Db>(Reuse.Singleton);
            App.Container.Register<Bot>(Reuse.Singleton);
            App.Container.Resolve<Bot>().Build();
        }
    }
}
