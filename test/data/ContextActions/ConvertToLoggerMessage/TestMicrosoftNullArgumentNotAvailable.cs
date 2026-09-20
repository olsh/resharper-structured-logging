using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger)
        {
            logger.{off}LogInformation("Order {OrderId} shipped", null);
        }
    }
}
