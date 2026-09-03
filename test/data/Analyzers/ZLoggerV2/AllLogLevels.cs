using Microsoft.Extensions.Logging;
using ZLogger;

namespace ConsoleApp
{
    class A
    {
        public A(ILogger<A> log, string host)
        {
            log.ZLogTrace($"Trace {host:@Host}");
            log.ZLogDebug($"Debug {host:@Host}");
            log.ZLogInformation($"Information {host:@Host}");
            log.ZLogWarning($"Warning {host:@Host}");
            log.ZLogError($"Error {host:@Host}");
            log.ZLogCritical($"Critical {host:@Host}");
            log.ZLog(LogLevel.Information, $"Explicit level {host:@hostName}");
        }
    }
}
