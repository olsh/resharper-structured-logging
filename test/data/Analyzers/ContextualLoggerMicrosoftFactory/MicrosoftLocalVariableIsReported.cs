using Microsoft.Extensions.Logging;

class A
{
    public void Configure(ILoggerFactory loggerFactory)
    {
        var log = loggerFactory.CreateLogger<B>();
    }
}

class B { }
