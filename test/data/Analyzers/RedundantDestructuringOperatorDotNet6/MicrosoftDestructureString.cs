using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public class Program
    {
        public Program(ILogger<Program> logger, string userName)
        {
            logger.LogInformation("User {@UserName} logged in", userName);
        }
    }
}
