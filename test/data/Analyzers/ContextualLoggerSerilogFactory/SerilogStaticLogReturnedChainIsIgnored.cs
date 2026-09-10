using Serilog;

static class WorkerLoggerFactory
{
    public static ILogger Create(int jobId)
    {
        return Log.ForContext<Worker>().ForContext("JobId", jobId);
    }
}

class Worker { }
