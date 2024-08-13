using DryIoc;
using RightVisionBotDb.Core;
using RightVisionBotDb.Core.Services;
using Serilog;

namespace RightVisionBotDb.Handler
{
    public class Handler
    {
        public Core.Core Core { get; set; }

        public void RegisterTypes()
        {
            Log.Logger?.Information("Регистрация типов и сервисов из {project}...", "RightVisionBotDb.Handler");

            App.Container.Register<Core.Core>(Reuse.Singleton);

            App.Container.Register<ProfileStringService>(Reuse.Singleton);

            App.OnConfiguringEnded += App_OnConfiguringEnded;

            App.Container.Resolve<Core.Core>().RegisterTypes();
        }

        private void App_OnConfiguringEnded(object? sender, EventArgs e)
        {
            Core = App.Container.Resolve<Core.Core>();
        }
    }
}
