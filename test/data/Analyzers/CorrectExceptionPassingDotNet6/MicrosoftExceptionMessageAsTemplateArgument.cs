using System;
using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, Exception exception)
        {
            logger.LogError("Import failed {Error}", exception.Message);
        }
    }
}
