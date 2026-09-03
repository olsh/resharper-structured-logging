using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [ZLoggerMessage(0, LogLevel.Information, "Could not open socket to {hostName}")]
        public static partial void CouldNotOpenSocket(ILogger logger, string hostName);
    }
}
