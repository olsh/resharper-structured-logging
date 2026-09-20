using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, object[] values)
        {
            logger.{off}LogInformation("Order {OrderId} shipped to {Country}", values);
        }
    }
}
