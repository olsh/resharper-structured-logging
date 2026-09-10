using Microsoft.Extensions.Logging;

class Worker
{
    public ILogger Logger { get; set; }
}

class A
{
    public void Configure(ILoggerFactory loggerFactory, Worker worker)
    {
        worker.Logger = loggerFactory.CreateLogger<Worker>();
    }
}
