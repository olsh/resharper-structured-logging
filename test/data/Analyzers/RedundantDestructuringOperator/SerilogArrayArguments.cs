using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("User {@UserName}", new object[] { "admin" });
        }
    }
}
