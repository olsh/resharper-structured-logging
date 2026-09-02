using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(DateTime timestamp)
        {
            Log.Logger.Information("Started at {0{caret}:HH:mm:ss}", timestamp);
        }
    }
}
