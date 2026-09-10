using Microsoft.Extensions.Logging;

static class WorkerLoggerFactory
{
    public static ILogger<Worker> Create(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger<Worker>();
    }
}

class Worker { }
