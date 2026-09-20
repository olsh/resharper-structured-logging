using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log, string userName)
        {
            log.ZLogInformation($"Logged in {userName:@UserName}");
        }
    }
}
