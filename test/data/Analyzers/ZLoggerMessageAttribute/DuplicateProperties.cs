using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [ZLoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "{Host} is not equal to {Host}")]
        public static partial void HostMismatch(ILogger logger, string host, string otherHost);
    }
}
