using Serilog;
using Serilog.Context;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            var s = "world";
            LogContext.PushProperty($"Hello{s}", 1);
        }
    }
}
