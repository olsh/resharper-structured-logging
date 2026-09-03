using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log, string first, string second)
        {
            log.ZLogInformation($"{first:@Host} is not equal to {second:@Host}");
        }
    }
}
