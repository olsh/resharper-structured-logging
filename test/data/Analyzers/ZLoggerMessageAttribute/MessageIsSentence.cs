using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [ZLoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Could not open socket to {HostName}.")]
        public static partial void CouldNotOpenSocket(ILogger logger, string hostName);
    }
}
