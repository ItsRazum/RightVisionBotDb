using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;

namespace RightVisionBotDb.Locations
{
    public abstract class RvLocation : IRvLocation
    {
        #region Properties

        public Dictionary<string, RvTextCommand> TextCommands { get; }
        public Dictionary<string, RvCallbackCommand> CallbackCommands { get; }

        protected Bot Bot { get; }
        protected Keyboards Keyboards { get; }
        protected LocationManager LocationManager { get; }
        protected RvLogger Logger { get; }
        protected LogMessages LogMessages { get; }
        protected LocationsFront LocationsFront { get; }

        #endregion

        #region Constructor

        public RvLocation(
            Bot bot,
            Keyboards keyboards,
            LocationManager locationManager,
            RvLogger logger,
            LogMessages logMessages,
            LocationsFront locationsFront)
        {
            Bot = bot ?? throw new ArgumentNullException(nameof(bot));
            Keyboards = keyboards ?? throw new ArgumentNullException(nameof(keyboards));
            LocationManager = locationManager ?? throw new ArgumentNullException(nameof(locationManager));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            LogMessages = logMessages ?? throw new ArgumentNullException(nameof(logMessages));
            LocationsFront = locationsFront ?? throw new ArgumentNullException(nameof(locationsFront));

            TextCommands = [];
            CallbackCommands = [];
        }

        #endregion

        #region IRvLocation implementation 

        public virtual async Task HandleCommandAsync(CommandContext c, bool containsArgs, CancellationToken token = default)
        {
            if (c.Message.Text != null)
            {
                if (containsArgs)
                {
                    var commandData = c.Message.Text!.Split(' ').First();
                    if (TextCommands.TryGetValue(commandData, out var command))
                    {
                        await command.ExecuteAsync(c, token);
                    }
                }
                else if (TextCommands.TryGetValue(c.Message.Text!, out var command))
                {
                    await command.ExecuteAsync(c, token);
                }
            }
        }

        public override string ToString()
        {
            return LocationManager.LocationToString(this);
        }

        public virtual async Task HandleCallbackAsync(CallbackContext c, bool containsArgs, CancellationToken token = default)
        {
            if (containsArgs)
            {
                var commandData = c.CallbackQuery.Data!.Split('-').First();
                if (CallbackCommands.TryGetValue(commandData, out var command))
                {
                    await command.ExecuteAsync(c, token);
                }
            }
            else if (CallbackCommands.TryGetValue(c.CallbackQuery.Data!, out var command))
            {
                await command.ExecuteAsync(c, token);
            }
        }

        public IRvLocation RegisterTextCommand(string command, Func<CommandContext, CancellationToken, Task> handler)
        {
            if(TextCommands.ContainsKey(command))
            {
                throw new InvalidOperationException($"Команда {command} уже зарегистрирована в локации {this}.");
            }
            TextCommands.Add(command, new RvTextCommand(handler));
            return this;
        }

        public IRvLocation RegisterCallbackCommand(string command, Func<CallbackContext, CancellationToken, Task> handler)
        {
            if (CallbackCommands.ContainsKey(command))
            {
                throw new InvalidOperationException($"Колбэк {command} уже зарегистрирован в локации {this}.");
            }
            CallbackCommands.Add(command, new RvCallbackCommand(handler));
            return this;
        }

        #endregion
    }
}
