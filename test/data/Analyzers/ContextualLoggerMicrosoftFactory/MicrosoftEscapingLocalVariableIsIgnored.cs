using Microsoft.Extensions.Logging;

class Worker
{
    public Worker(ILogger<Worker> logger)
    {
    }
}

class A
{
    public Worker Create(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Worker>();

        return new Worker(logger);
    }
}
