using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            new LoggerConfiguration().CreateLogger().ForContext("Test", new Random());
        }
    }
}
