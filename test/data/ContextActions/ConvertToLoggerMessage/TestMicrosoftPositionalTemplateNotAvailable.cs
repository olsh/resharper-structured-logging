using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, int quota, string user)
        {
            logger.{off}LogError("Disk quota {0} exceeded by {1}", quota, user);
        }
    }
}
