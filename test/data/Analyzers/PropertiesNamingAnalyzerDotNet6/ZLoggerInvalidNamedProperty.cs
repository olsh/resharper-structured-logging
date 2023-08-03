using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log)
        {
            log.ZLogInformation("{myProperty}", 1);
        }
    }
}
