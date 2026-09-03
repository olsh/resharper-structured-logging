using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(Exception exception)
        {
            Log.Logger.Error(exception, "Disk quota {Quota} MB exceeded {Other}", 100, {caret}new Exception("second"));
        }
    }
}
