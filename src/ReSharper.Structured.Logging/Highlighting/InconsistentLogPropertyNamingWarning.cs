using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;

using ReSharper.Structured.Logging.Models;
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
            MessageTemplateTokenInformation tokenInformation,
            PropertyToken namedProperty,
            string suggestedName)
        {
            TokenInformation = tokenInformation;
            NamedProperty = namedProperty;
            SuggestedName = suggestedName;
        }

        public string ErrorStripeToolTip => ToolTip;

        public MessageTemplateTokenInformation TokenInformation { get; }

        public PropertyToken NamedProperty { get; }

        public string SuggestedName { get; }

        public string ToolTip => $"Property name '{NamedProperty.PropertyName}' does not naming rules'. Suggested name is '{SuggestedName}'.";

        public DocumentRange CalculateRange()
        {
            return TokenInformation.DocumentRange;
        }

        public bool IsValid()
        {
            return TokenInformation.DocumentRange.IsValid();
        }
    }
}
