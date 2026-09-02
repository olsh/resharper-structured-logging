using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public static class Program
    {
        public static void Main(ILogger logger, Options options)
        {
            logger.LogInformation($"Text read from settings: '{options.Val{caret}ue.Text}'");
        }
    }

    public class Options
    {
        public Settings Value { get; set; }
    }

    public class Settings
    {
        public string Text { get; set; }
    }
}
