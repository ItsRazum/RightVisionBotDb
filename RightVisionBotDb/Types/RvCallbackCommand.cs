﻿using RightVisionBotDb.Enums;

namespace RightVisionBotDb.Types
{
    public class RvCallbackCommand
    {
        private readonly Func<CallbackContext, CancellationToken, Task> _executeMethod;
        private readonly Permission? _requiredPermission;

        public RvCallbackCommand(Func<CallbackContext, CancellationToken, Task> executeMethod, Permission? requiredPermission = null)
        {
            _executeMethod = executeMethod;
            _requiredPermission = requiredPermission;
        }

        public async Task<bool> ExecuteAsync(CallbackContext c, CancellationToken token = default)
        {
            if (c.RvUser.Has(Permission.Messaging))
            {
                if (_requiredPermission == null || c.RvUser.Has((Permission)_requiredPermission))
                {
                    await _executeMethod(c, token);
                    return true;
                }
            }
            return false;
        }
    }
}
