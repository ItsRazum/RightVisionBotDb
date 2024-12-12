using RightVisionBotDb.Enums;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Text;
using RightVisionBotDb.Types;
using System.Text.RegularExpressions;
using Telegram.Bot;

namespace RightVisionBotDb.Locations
{
    public abstract class RvLocation
    {
        #region Properties

        protected Dictionary<string, RvTextCommand> TextCommands { get; }
        protected Dictionary<string, RvCallbackCommand> CallbackCommands { get; }
        protected Bot Bot { get; }
        protected LocationService LocationService { get; }
        protected RvLogger RvLogger { get; }
        protected LocationsFront LocationsFront { get; }

        #endregion

        #region Constructor

        protected RvLocation(
            Bot bot,
            LocationService locationService,
            RvLogger logger,
            LocationsFront locationsFront)
        {
            Bot = bot;
            LocationService = locationService;
            RvLogger = logger;
            LocationsFront = locationsFront;

            TextCommands = [];
            CallbackCommands = [];
        }

        #endregion

        #region Methods 

        public virtual async Task HandleCommandAsync(CommandContext c, bool containsArgs, CancellationToken token = default)
        {
            if (c.Message.Text != null)
            {
                if (containsArgs)
                    c.Message.Text = Regex.Replace(c.Message.Text, @"\s{2,}", " ").Trim();

                var commandData = (containsArgs ? c.Message.Text.Split(' ').First() : c.Message.Text).ToLower();

                if (TextCommands.TryGetValue(commandData, out var command) && !await command.ExecuteAsync(c, token))
                    await Bot.Client.SendTextMessageAsync(c.Message.Chat, Phrases.Lang[c.RvUser.Lang].Messages.Common.NoPermission, cancellationToken: token);
            }
        }

        public override string ToString()
        {
            return LocationService.LocationToString(this);
        }

        public virtual async Task HandleCallbackAsync(CallbackContext c, bool containsArgs, CancellationToken token = default)
        {
            var commandData = containsArgs ? c.CallbackQuery.Data!.Split('-').First() : c.CallbackQuery.Data!;

            if (CallbackCommands.TryGetValue(commandData, out var command) && !await command.ExecuteAsync(c, token))
                await Bot.Client.AnswerCallbackQueryAsync(c.CallbackQuery.Id, Phrases.Lang[c.RvUser.Lang].Messages.Common.NoPermission, showAlert: true, cancellationToken: token);
        }

        public RvLocation RegisterTextCommand(string command, Func<CommandContext, CancellationToken, Task> handler, Permission? requiredPermission = null)
        {
            if (TextCommands.ContainsKey(command))
            {
                throw new InvalidOperationException($"Команда {command} уже зарегистрирована в локации {this}.");
            }
            var rvTextCommand = new RvTextCommand(handler, requiredPermission);
            TextCommands.Add(command, rvTextCommand);

            if (command.StartsWith('/'))
            {
                command += "@rightvisionbot";
                TextCommands.Add(command, rvTextCommand);
            }
            return this;
        }

        public RvLocation RegisterCallbackCommand(string command, Func<CallbackContext, CancellationToken, Task> handler, Permission? requiredPermission = null)
        {
            if (CallbackCommands.ContainsKey(command))
            {
                throw new InvalidOperationException($"Колбэк {command} уже зарегистрирован в локации {this}.");
            }
            CallbackCommands.Add(command, new RvCallbackCommand(handler, requiredPermission));
            return this;
        }

        public RvLocation RegisterTextCommands(IEnumerable<string> commands, Func<CommandContext, CancellationToken, Task> handler, Permission? requiredPermission = null)
        {
            foreach (var command in commands)
                RegisterTextCommand(command, handler, requiredPermission);

            return this;
        }

        public RvLocation RegisterCallbackCommands(IEnumerable<string> commands, Func<CallbackContext, CancellationToken, Task> handler, Permission? requiredPermission = null)
        {
            foreach (var command in commands)
                RegisterCallbackCommand(command, handler, requiredPermission);

            return this;
        }

        #endregion
    }
}
