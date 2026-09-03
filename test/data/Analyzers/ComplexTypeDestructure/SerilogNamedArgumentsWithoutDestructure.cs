using System;
using Serilog;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Log.Logger.Information(propertyValue0: new Random(), messageTemplate: "{First} {Second}", propertyValue1: 1);
        }
    }
}
