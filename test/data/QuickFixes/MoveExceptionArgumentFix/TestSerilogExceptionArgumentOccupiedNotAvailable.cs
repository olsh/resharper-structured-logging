using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(Exception exception, Exception other, int quota)
        {
            Log.Logger.Error(other, "Disk quota {Quota} MB exceeded {Exception}", quota, {caret}exception);
        }
    }
}
