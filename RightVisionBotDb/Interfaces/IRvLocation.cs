﻿using RightVisionBotDb.Enums;
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

        IRvLocation RegisterTextCommand(string command, Func<CommandContext, CancellationToken, Task> handler, Permission? requiredPermission = null);
        IRvLocation RegisterCallbackCommand(string command, Func<CallbackContext, CancellationToken, Task> handler, Permission? requiredPermission = null);

        IRvLocation RegisterTextCommands(IEnumerable<string> commands, Func<CommandContext, CancellationToken, Task> handler, Permission? requiredPermission = null);
        IRvLocation RegisterCallbackCommands(IEnumerable<string> commands, Func<CallbackContext, CancellationToken, Task> handler, Permission? requiredPermission = null);
    }
}
