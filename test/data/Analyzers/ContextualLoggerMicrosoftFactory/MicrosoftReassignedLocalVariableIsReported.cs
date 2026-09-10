using Microsoft.Extensions.Logging;

class A
{
    public void Configure(ILoggerFactory loggerFactory)
    {
        ILogger log = loggerFactory.CreateLogger<B>();
        log = loggerFactory.CreateLogger<A>();

        log.LogInformation("Configured");
    }
}

class B { }
