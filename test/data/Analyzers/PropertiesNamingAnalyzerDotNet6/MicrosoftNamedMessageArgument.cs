using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log)
        {
            log.LogInformation(message: "{myProperty}", 1);
        }
    }
}
