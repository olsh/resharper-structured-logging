using System;

using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            ILogger logger = null;
            Exception exception = null;
            logger.LogError(exception, message: "{caret}database update failed.");
        }
    }
}
