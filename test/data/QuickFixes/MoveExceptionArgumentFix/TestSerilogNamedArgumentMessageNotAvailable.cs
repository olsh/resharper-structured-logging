using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(Exception exception, int quota)
        {
            Log.Logger.Error(propertyValue0: quota, propertyValue1: {caret}exception.Message, messageTemplate: "Disk quota {Quota} MB exceeded {Error}");
        }
    }
}
