using Microsoft.Extensions.Logging;

class A
{
    private readonly ILogger _log;

    public A(ILoggerFactory loggerFactory)
    {
        _log = loggerFactory.CreateLogger<B>();
    }
}

class B { }
