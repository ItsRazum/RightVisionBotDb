using RightVisionBotDb.Data;
using RightVisionBotDb.Models;
using System;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Types
{
    public class RvCallbackCommand
    {
        private readonly Func<CallbackContext, CancellationToken, Task> _executeMethod;

        public RvCallbackCommand(Func<CallbackContext, CancellationToken, Task> executeMethod) 
        {
            _executeMethod = executeMethod;
        }

        public async Task ExecuteAsync(CallbackContext c, CancellationToken token = default) 
        {
            await _executeMethod(c, token);
        }
    }
}
