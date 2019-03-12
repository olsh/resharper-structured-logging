using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information("{0}", 1);
        }
    }
}
