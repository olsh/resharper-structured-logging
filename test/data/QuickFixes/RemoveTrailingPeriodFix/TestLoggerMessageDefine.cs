using System;

using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class LogMessages
    {
        private static readonly Action<ILogger, Exception> CouldNotOpenSocket =
            LoggerMessage.Define(LogLevel.Information, new EventId(0), "{caret}Could not open socket.");
    }
}
