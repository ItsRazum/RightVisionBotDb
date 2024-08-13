using DryIoc;
using Serilog;

namespace RightVisionBotDb.Bot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Logger?.Information("================= RightVisionBot =================");

            App.Container.RegisterInstance(Log.Logger);

            App.Container.Register<Bot>(Reuse.Singleton);

            App.Container.Resolve<Bot>().RegisterTypes();

            Console.ReadLine();
        }
    }
}
