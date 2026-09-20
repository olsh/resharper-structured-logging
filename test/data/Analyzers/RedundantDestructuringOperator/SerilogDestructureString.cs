using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(string userName)
        {
            Log.Logger.Information("User {@UserName} logged in", userName);
        }
    }
}
