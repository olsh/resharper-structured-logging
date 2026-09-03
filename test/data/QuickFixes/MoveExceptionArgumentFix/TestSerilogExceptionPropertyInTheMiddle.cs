using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(Exception exception, int quota)
        {
            Log.Logger.Error("Disk quota {Exception} MB exceeded {Quota}", {caret}exception, quota);
        }
    }
}
