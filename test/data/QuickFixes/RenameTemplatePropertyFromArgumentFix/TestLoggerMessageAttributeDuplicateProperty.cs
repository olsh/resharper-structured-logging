using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "{Host} is not equal to {Ho{caret}st}")]
        public static partial void HostMismatch(ILogger logger, string host, string otherHost);
    }
}
