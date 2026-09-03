using Microsoft.Extensions.Logging;

class A
{
    public void Configure(ILoggerFactory loggerFactory)
    {
        ILogger<B> log = loggerFactory.CreateLogger<{caret}B>();
    }
}

class B { }
