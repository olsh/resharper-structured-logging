using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public class Endpoint
    {
        public string Host { get; set; }
    }

    public static partial class LogMessages
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Could not open socket to {@Endpoint}")]
        public static partial void CouldNotOpenSocket(ILogger logger, Endpoint endpoint);
    }
}
