namespace ReSharper.Structured.Logging.Tests.Constants
{
    internal static class NugetPackages
    {
        public const string SerilogNugetPackage = "Serilog/2.7.1";

        public const string MicrosoftLoggingPackage = "Microsoft.Extensions.Logging/6.0.0";

        public const string NlogLoggingPackage = "NLog/6.2.0";

        // ZLogger 1.x for the ZLogInformation(string, params object[]) overloads
        public const string ZLoggerLoggingPackage = "ZLogger/1.7.0";

        // ZLogger 2.x replaced those overloads with interpolated string handlers, and is also the only
        // version that has ZLoggerMessageAttribute
        public const string ZLoggerV2LoggingPackage = "ZLogger/2.5.10";
    }
}
