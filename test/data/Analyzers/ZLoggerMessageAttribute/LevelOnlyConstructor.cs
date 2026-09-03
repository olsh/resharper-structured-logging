using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [ZLoggerMessage(LogLevel.Information)]
        public static partial void CouldNotOpenSocket(ILogger logger);
    }
}
