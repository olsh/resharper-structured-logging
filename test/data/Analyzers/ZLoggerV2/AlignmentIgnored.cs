using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log, int retryCount)
        {
            log.ZLogInformation($"Retried {retryCount,10} times");
        }
    }
}
