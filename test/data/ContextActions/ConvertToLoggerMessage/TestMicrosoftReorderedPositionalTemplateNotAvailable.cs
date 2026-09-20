using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, string first, string second)
        {
            logger.{off}LogInformation("{1} {0}", first, second);
        }
    }
}
