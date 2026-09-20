using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Shipped {{caret}")]
        public static partial void Shipped(ILogger logger, int orderId);
    }
}
