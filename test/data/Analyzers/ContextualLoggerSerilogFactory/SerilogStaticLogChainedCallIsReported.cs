using Serilog;

class A
{
    private static readonly ILogger Logger = Log.ForContext<B>().ForContext("JobId", 1);
}

class B { }
