using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [ZLoggerMessage(EventId = 0, EventName = "{eventName}", Level = LogLevel.Information, Message = "Could not open socket")]
        public static partial void CouldNotOpenSocket(ILogger logger);
    }
}
