using RightVisionBotDb.Enums;

namespace RightVisionBotDb.Types
{
    public class RvTextCommand
    {
        private readonly Func<CommandContext, CancellationToken, Task> _executeMethod;
        private readonly Permission? _requiredPermission;

        public RvTextCommand(Func<CommandContext, CancellationToken, Task> executeMethod, Permission? requiredPermission = null)
        {
            _executeMethod = executeMethod;
            _requiredPermission = requiredPermission;
        }

        public async Task<bool> ExecuteAsync(CommandContext commandContext, CancellationToken token = default)
        {
            if(commandContext.RvUser.Has(Permission.Messaging))
            {
                if (_requiredPermission == null || commandContext.RvUser.Has((Permission)_requiredPermission))
                {
                    await _executeMethod(commandContext, token);
                    return true;
                }
            }
            return false;
        }
    }
}
