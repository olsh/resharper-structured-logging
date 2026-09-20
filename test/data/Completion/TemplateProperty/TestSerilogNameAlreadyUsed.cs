using Serilog;

namespace ConsoleApp
{
    public class Invoice
    {
        public int Id { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
    }

    public static class Program
    {
        public static void Main(Order order, Invoice invoice)
        {
            Log.Logger.Information("Shipped {Id} with {{caret}", order.Id, invoice.Id);
        }
    }
}
