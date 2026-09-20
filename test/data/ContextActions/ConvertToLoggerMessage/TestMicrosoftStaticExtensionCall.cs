using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, string host)
        {
            LoggerExtensions.{caret}LogTrace(logger, "Connecting to {Host}", host);
        }
    }
}
