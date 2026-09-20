using Serilog;

namespace ConsoleApp
{
    public class Notification
    {
        public string Message { get; set; }
    }

    public static class Program
    {
        public static void Main(Notification notification)
        {
            Log.Logger.Error("Import failed {Error}", notification.Message);
        }
    }
}
