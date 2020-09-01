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
    public class InconsistentLogPropertyNamingWarning : IHighlighting
    {
        public const string Message = "Property name '{0}' does not naming rules'. Suggested name is '{1}'.";

        public const string SeverityId = "InconsistentLogPropertyNaming";

        public InconsistentLogPropertyNamingWarning(
            IStringLiteralAlterer stringLiteral,
            string suggestedName,
            PropertyToken namedProperty,
            DocumentRange documentRange)
        {
            StringLiteral = stringLiteral;
            SuggestedName = suggestedName;
            NamedProperty = namedProperty;
            Range = documentRange;
        }

        public string ErrorStripeToolTip => ToolTip;

        public PropertyToken NamedProperty { get; }

        public DocumentRange Range { get; }

        public IStringLiteralAlterer StringLiteral { get; }

        public string SuggestedName { get; }

        public string ToolTip => $"Property name '{NamedProperty.PropertyName}' does not naming rules'. Suggested name is '{SuggestedName}'.";

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
