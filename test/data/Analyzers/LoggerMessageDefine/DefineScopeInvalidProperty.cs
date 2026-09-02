using System;

using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class LogMessages
    {
        private static readonly Func<ILogger, string, IDisposable> ProcessingScope =
            LoggerMessage.DefineScope<string>("Processing {requestId}");
    }
}
