using Microsoft.Extensions.Logging;

class A
{
    ILogger<B> _log;

    public A(ILoggerFactory loggerFactory)
    {
        _log = loggerFactory.CreateLogger<{caret}B>();
    }
}

class B { }
