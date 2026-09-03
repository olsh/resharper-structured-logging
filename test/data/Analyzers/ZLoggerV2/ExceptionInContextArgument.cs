using System;

using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log, Exception exception)
        {
            log.ZLogInformation($"Could not open socket", exception);
        }
    }
}
