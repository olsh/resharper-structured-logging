using Serilog;

class A
{
    private static ILogger Logger { get; } = Log.ForContext<B>();
}

class B { }
