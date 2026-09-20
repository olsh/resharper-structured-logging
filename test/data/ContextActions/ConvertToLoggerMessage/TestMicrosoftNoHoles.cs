using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger)
        {
            logger.{caret}LogDebug("Cache warmed up");
        }
    }
}
