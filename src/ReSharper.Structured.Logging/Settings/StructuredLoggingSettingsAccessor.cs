using System;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace ReSharper.Structured.Logging.Settings
{
    public static class StructuredLoggingSettingsAccessor
    {
        [NotNull]
        public static readonly Expression<Func<StructuredLoggingSettings, PropertyNamingType>> PropertyNamingType = x => x.PropertyNamingType;

        [NotNull]
        public static readonly Expression<Func<StructuredLoggingSettings, string>> IgnoredPropertiesRegex = x => x.IgnoredPropertiesRegex;
    }
}
