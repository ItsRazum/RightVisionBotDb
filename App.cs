using DryIoc;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions;
using Serilog.Extensions.Logging;

namespace RightVisionBotDb
{
    public class App
    {
        public static readonly IContainer Container = new Container();
    }
}
