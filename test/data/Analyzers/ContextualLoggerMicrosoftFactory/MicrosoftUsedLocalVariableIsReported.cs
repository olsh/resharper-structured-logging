using Microsoft.Extensions.Logging;

class A
{
    public void Configure(ILoggerFactory loggerFactory)
    {
        var log = loggerFactory.CreateLogger<B>();

        log.LogInformation("Configured");
    }
}

class B { }
