using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(string orderId)
        {
            Log.Logger.Information("Order {$Order{caret}Id} processed", orderId);
        }
    }
}
