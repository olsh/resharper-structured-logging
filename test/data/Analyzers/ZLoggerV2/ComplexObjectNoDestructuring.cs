using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class User
    {
        public string Name { get; set; }
    }

    class A
    {
        public A(ILogger<A> log, User user)
        {
            log.ZLogInformation($"Logged in {user:@User}");
        }
    }
}
