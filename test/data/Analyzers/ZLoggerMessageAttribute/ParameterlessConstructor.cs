using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [ZLoggerMessage]
        public static partial void CouldNotOpenSocket(ILogger logger, LogLevel level);
    }
}
