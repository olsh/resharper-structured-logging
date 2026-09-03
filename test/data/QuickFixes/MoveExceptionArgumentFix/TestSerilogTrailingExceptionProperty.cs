using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(Exception exception, int quota)
        {
            Log.Logger.Error("Disk quota {Quota} MB exceeded {Exception}", quota, {caret}exception);
        }
    }
}
