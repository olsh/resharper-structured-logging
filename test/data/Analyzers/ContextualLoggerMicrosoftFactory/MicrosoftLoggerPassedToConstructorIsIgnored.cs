using Microsoft.Extensions.Logging;

class Worker
{
    public Worker(ILogger<Worker> logger)
    {
    }
}

static class WorkerFactory
{
    public static Worker Create(ILoggerFactory loggerFactory)
    {
        return new Worker(loggerFactory.CreateLogger<Worker>());
    }
}
