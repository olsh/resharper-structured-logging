using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "{caret}Could not open socket.")]
        public static partial void CouldNotOpenSocket(ILogger logger);
    }
}
