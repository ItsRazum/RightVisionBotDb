namespace RightVisionBotDb.Types
{
    public class RvTextCommand
    {
        private readonly Func<CommandContext, CancellationToken, Task> _executeMethod;

        public RvTextCommand(Func<CommandContext, CancellationToken, Task> executeMethod)
        {
            _executeMethod = executeMethod;
        }

        public async Task ExecuteAsync(CommandContext commandContext, CancellationToken token = default)
        {
            await _executeMethod(commandContext, token);
        }
    }
}
