using System;
using Serilog;
using Serilog.Context;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            LogContext.PushProperty("Test", new Random());
        }
    }
}
