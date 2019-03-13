using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Serilog.Parsing;

[assembly:
    RegisterConfigurableSeverity(
        AnonymousObjectDestructuringWarning.SeverityId,
        null,
        HighlightingGroupIds.CompilerWarnings,
        AnonymousObjectDestructuringWarning.Message,
        AnonymousObjectDestructuringWarning.Message,
        Severity.WARNING)]

namespace ReSharper.Structured.Logging.Highlighting
{
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        OverlapResolve = OverlapResolveKind.WARNING,
        ToolTipFormatString = Message)]
    public class AnonymousObjectDestructuringWarning : IHighlighting
    {
        public const string Message = "Anonymous objects must be destructured";

        public const string SeverityId = "AnonymousObjectDestructuringProblem";

        public AnonymousObjectDestructuringWarning(
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
