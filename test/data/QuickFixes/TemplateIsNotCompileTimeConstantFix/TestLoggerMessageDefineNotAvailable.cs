using System;

using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class LogMessages
    {
        public static Action<ILogger, Exception> Create(string host)
        {
            return LoggerMessage.Define(LogLevel.Information, new EventId(0), $"Could not open socket to {ho{caret}st}");
        }
    }
}
