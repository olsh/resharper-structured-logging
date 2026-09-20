using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, int retries)
        {
            logger.{caret}Log(LogLevel.Critical, "Retried {Retries} times", retries);
        }
    }
}
