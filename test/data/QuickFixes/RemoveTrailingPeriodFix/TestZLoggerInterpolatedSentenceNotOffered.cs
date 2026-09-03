using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log, string host)
        {
            log.ZLogInformation($"Could not open {caret}socket to {host:@Host}.");
        }
    }
}
