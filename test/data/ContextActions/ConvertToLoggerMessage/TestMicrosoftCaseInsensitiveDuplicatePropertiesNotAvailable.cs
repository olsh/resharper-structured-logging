using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, string url, string otherUrl)
        {
            logger.{off}LogInformation("Fetched {URL} after {Url}", url, otherUrl);
        }
    }
}
