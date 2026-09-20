using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(int orderId)
        {
            Log.Logger.{off}Information("Order {OrderId} shipped", orderId);
        }
    }
}
