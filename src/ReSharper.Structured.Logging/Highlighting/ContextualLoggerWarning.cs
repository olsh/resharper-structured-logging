using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;

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
    public class ContextualLoggerWarning : IHighlighting
    {
        private const string Message = "Incorrect type is used for contextual logger";

        public const string SeverityId = "ContextualLoggerProblem";

        private readonly DocumentRange _range;

        public ContextualLoggerWarning(DocumentRange documentRange)
        {
            _range = documentRange;
        }

        public string ErrorStripeToolTip => ToolTip;

        public string ToolTip => Message;

        public DocumentRange CalculateRange()
        {
            return _range;
        }

        public bool IsValid()
        {
            return _range.IsValid();
        }
    }
}
