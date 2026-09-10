using Serilog;

class A
{
    public void Run()
    {
        Log.ForContext<B>().Information("Started");
    }
}

class B { }
