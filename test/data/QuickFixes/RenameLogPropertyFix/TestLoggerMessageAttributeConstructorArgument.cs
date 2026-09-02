using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static partial class LogMessages
    {
        [LoggerMessage(0, LogLevel.Information, "Retrying {att{caret}empt}")]
        public static partial void Retry(ILogger logger, int attempt);
    }
}
