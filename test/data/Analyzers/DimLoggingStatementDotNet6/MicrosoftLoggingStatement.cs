using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public class Service
    {
        private readonly ILogger<Service> _logger;

        public Service(ILogger<Service> logger)
        {
            _logger = logger;
        }

        public void Connect(string host, int port)
        {
            _logger.LogInformation(
                "Connecting to {Host} on {Port}",
                host,
                port);
        }
    }
}
