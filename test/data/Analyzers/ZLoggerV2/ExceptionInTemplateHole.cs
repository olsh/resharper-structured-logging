using System;

using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log, Exception exception)
        {
            log.ZLogError($"Could not open socket {exception:@Error}");
        }
    }
}
