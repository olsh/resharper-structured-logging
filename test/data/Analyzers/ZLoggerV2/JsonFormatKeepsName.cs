using System;

using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log, Uri uri)
        {
            log.ZLogInformation($"Connected to {uri:json}");
        }
    }
}
