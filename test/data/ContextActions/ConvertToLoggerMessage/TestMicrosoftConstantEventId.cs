using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, string userName)
        {
            logger.{caret}LogWarning(42, "User {UserName} was throttled", userName);
        }
    }
}
