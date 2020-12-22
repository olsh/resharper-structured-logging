using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;

using ReSharper.Structured.Logging.Models;
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
    public class ComplexObjectDestructuringWarning : IHighlighting
    {
        public const string Message = "Complex objects with default ToString() implementation probably need to be destructured";

        public const string SeverityId = "ComplexObjectDestructuringProblem";

        public ComplexObjectDestructuringWarning(MessageTemplateTokenInformation tokenInformation)
        {
            TokenInformation = tokenInformation;
        }

        public string ErrorStripeToolTip => ToolTip;

        public MessageTemplateTokenInformation TokenInformation { get; }

        public string ToolTip => Message;

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
