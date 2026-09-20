using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, LogLevel level, int retries)
        {
            logger.{caret}Log(level, "Retried {Retries} times", retries);
        }
    }
}
