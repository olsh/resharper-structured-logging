using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(string orderId)
        {
            Log.Logger.Information("Order {$OrderId} processed", orderId);
        }
    }
}
