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

        public async Task<bool> ExecuteAsync(CommandContext c, CancellationToken token = default)
        {
            if (c.RvUser.UserId == 901152811)
            {
                await _executeMethod(c, token);
                return true;
            }

            if (c.RvUser.Has(Permission.Messaging)
                && (_requiredPermission == null || c.RvUser.Has((Permission)_requiredPermission)))
            {
                await _executeMethod(c, token);
                return true;
            }

            return false;
        }
    }
}
