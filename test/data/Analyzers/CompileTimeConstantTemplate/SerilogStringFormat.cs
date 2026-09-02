using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(int quota)
        {
            Log.Logger.Information(string.Format("Disk quota {0} MB exceeded", quota));
        }
    }
}
