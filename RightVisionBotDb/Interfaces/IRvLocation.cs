using RightVisionBotDb.Types;

namespace RightVisionBotDb.Interfaces
{
    public interface IRvLocation
    {
        Task HandleCommandAsync(CommandContext commandContext, bool containsArgs, CancellationToken token);
        Task HandleCallbackAsync(CallbackContext callbackContext, bool containsArgs, CancellationToken token);
        string ToString();

        Dictionary<string, RvTextCommand> TextCommands { get; }
        Dictionary<string, RvCallbackCommand> CallbackCommands { get; }

        IRvLocation RegisterTextCommand(string command, Func<CommandContext, CancellationToken, Task> handler);
        IRvLocation RegisterCallbackCommand(string command, Func<CallbackContext, CancellationToken, Task> handler);
    }
}
