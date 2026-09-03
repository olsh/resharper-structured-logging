using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(string host)
        {
            Log.Logger.Information("Could not open socket to " + host);
        }
    }
}
