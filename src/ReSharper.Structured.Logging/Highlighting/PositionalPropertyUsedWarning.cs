using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Serilog.Parsing;
using ReSharper.Structured.Logging.Settings;

namespace ReSharper.Structured.Logging.Highlighting
{
    [RegisterConfigurableSeverity(
        SeverityId,
        null,
        StructuredLoggingGroup.Id,
        Message,
        Message,
        Severity.WARNING)]
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        OverlapResolve = OverlapResolveKind.WARNING,
        ToolTipFormatString = Message)]
    public class PositionalPropertyUsedWarning : IHighlighting
    {
        public const string Message = "Prefer named properties instead of positional ones";

        public const string SeverityId = "PositionalPropertyUsedProblem";

        public PositionalPropertyUsedWarning(
            IStringLiteralAlterer stringLiteral,
            PropertyToken namedProperty,
            DocumentRange documentRange)
        {
            StringLiteral = stringLiteral;
            NamedProperty = namedProperty;
            Range = documentRange;
        }

        public string ErrorStripeToolTip => ToolTip;

        public PropertyToken NamedProperty { get; }

        public DocumentRange Range { get; }

        public IStringLiteralAlterer StringLiteral { get; }

        public string ToolTip => Message;

        public DocumentRange CalculateRange()
        {
            return Range;
        }

        public bool IsValid()
        {
            return Range.IsValid();
        }
    }
}
