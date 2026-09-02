using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [LoggerMessage(EventId = 0, EventName = "{eventName}", Level = LogLevel.Information, Message = "Could not open socket")]
        public static partial void CouldNotOpenSocket(ILogger logger);
    }
}
