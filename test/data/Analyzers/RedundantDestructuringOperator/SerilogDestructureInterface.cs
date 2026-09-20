using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(IDisposable resource)
        {
            Log.Logger.Information("Resource {@Resource}", resource);
        }
    }
}
