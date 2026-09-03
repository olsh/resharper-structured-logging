using System;

using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log, Uri uri)
        {
            log.ZLogInformation($"{uri.Host} is not equal to {uri.Host}");
        }
    }
}
