using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(string template, int quota)
        {
            Log.Logger.Information(template, quota);
        }
    }
}
