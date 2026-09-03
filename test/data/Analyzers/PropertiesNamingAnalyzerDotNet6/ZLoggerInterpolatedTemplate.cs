using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log, string myProperty)
        {
            log.ZLogInformation($"Connected to {myProperty}", 1);
        }
    }
}
