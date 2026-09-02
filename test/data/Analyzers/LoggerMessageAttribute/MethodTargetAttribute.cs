using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [method: LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Could not open socket to {hostName}")]
        public static partial void CouldNotOpenSocket(ILogger logger, string hostName);
    }
}
