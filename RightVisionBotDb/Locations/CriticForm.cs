using RightVisionBotDb.Lang;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;

namespace RightVisionBotDb.Locations
{
    internal class CriticForm : RvLocationBase
    {
        public CriticForm(
            Bot bot, 
            Keyboards keyboards, 
            LocationManager locationManager, 
            RvLogger logger, 
            LogMessages logMessages,
            LocationsFront locationsFront)
            : base(bot, keyboards, locationManager, logger, logMessages, locationsFront)
        {
            foreach (var lang in App.RegisteredLangs)
                RegisterTextCommand(Language.Phrases[lang].KeyboardButtons.Back, BackCommand);
        }

        public override async Task HandleCommandAsync(CommandContext c, bool containsArgs, CancellationToken token = default)
        {
            await base.HandleCommandAsync(c, containsArgs, token);
            
        }

        private async Task BackCommand(CommandContext c, CancellationToken token = default)
        {

        }
    }
}
