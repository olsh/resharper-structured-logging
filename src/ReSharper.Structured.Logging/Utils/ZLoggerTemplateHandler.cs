using System;

using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;

namespace ReSharper.Structured.Logging.Services
{
    /// <summary>
    /// ZLogger 2.x carries the message template in an interpolated string handler instead of a string:
    /// every <c>ZLog*</c> overload declares one of the <c>ZLogger*InterpolatedStringHandler</c> ref structs.
    /// They are generated per log level, so they are matched by namespace and name shape rather than listed.
    /// </summary>
    public static class ZLoggerTemplateHandler
    {
        private const string NamespacePrefix = "ZLogger.";

        private const string TypeNameSuffix = "InterpolatedStringHandler";

        public static bool IsHandlerType([CanBeNull] IClrTypeName clrTypeName)
        {
            return clrTypeName != null
                   && clrTypeName.FullName.StartsWith(NamespacePrefix, StringComparison.Ordinal)
                   && clrTypeName.ShortName.EndsWith(TypeNameSuffix, StringComparison.Ordinal);
        }
    }
}
