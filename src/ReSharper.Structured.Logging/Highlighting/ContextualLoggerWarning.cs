using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;

using ReSharper.Structured.Logging.Highlighting;

[assembly:
    RegisterConfigurableSeverity(
        ContextualLoggerWarning.SeverityId,
        null,
        HighlightingGroupIds.CompilerWarnings,
        ContextualLoggerWarning.Message,
        ContextualLoggerWarning.Message,
        Severity.WARNING)]

namespace ReSharper.Structured.Logging.Highlighting
{
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        OverlapResolve = OverlapResolveKind.WARNING,
        ToolTipFormatString = Message)]
    public class ContextualLoggerWarning : IHighlighting
    {
        public const string Message = "Incorrect type is used for contextual logger";

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
