using System;

using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class LogMessages
    {
        private static readonly Action<ILogger, string, string, Exception> HostMismatch =
            LoggerMessage.Define<string, string>(LogLevel.Warning, new EventId(1), "{Host} is not equal to {Host}");
    }
}
