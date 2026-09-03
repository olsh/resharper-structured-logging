using Serilog;

class A
{
    private static readonly ILogger ContextLogger = Log.ForContext<{caret}B>();
}

class B { }
