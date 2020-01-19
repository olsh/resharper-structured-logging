using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Util;
using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Serilog.Parsing;

[assembly:
    RegisterConfigurableSeverity(
        ComplexObjectDestructuringWarning.SeverityId,
        null,
        HighlightingGroupIds.CompilerWarnings,
        ComplexObjectDestructuringWarning.Message,
        ComplexObjectDestructuringWarning.Message,
        Severity.WARNING)]

namespace ReSharper.Structured.Logging.Highlighting
{
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        OverlapResolve = OverlapResolveKind.WARNING,
        ToolTipFormatString = Message)]
    public class ComplexObjectDestructuringWarning : IHighlighting
    {
        public const string Message = "Complex objects with default ToString() implementation probably need to be destructured";

        public const string SeverityId = "ComplexObjectDestructuringProblem";

        public ComplexObjectDestructuringWarning(
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
