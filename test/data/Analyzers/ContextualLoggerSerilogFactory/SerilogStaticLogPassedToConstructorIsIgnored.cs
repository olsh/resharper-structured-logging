using Serilog;

class Worker
{
    public Worker(ILogger logger)
    {
    }
}

static class WorkerFactory
{
    public static Worker Create()
    {
        return new Worker(Log.ForContext<Worker>());
    }
}
