using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [LoggerMessage(0, LogLevel.Information, "Could not open socket to {hostName}")]
        public static partial void CouldNotOpenSocket(ILogger logger, string hostName);
    }
}
