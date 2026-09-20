using NLog;

namespace ConsoleApp
{
    public class Order
    {
        public int Id { get; set; }
    }

    public static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Main(Order order)
        {
            Log.Info("Shipped {{caret}", order.Id);
        }
    }
}
