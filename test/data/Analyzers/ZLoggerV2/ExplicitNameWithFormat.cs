using System;

using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log, DateTime startedAt)
        {
            log.ZLogInformation($"Started at {startedAt:@startedAtUtc:yyyy-MM-dd}");
        }
    }
}
