using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, decimal amount)
        {
            logger.{caret}LogInformation("Payment captured for {Amount,10:000}", amount);
        }
    }
}
