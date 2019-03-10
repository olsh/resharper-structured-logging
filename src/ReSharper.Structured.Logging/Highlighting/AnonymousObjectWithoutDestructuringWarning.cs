using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;

using ReSharper.Structured.Logging.Highlighting;

[assembly:
    RegisterConfigurableSeverity(AnonymousObjectDestructuringWarning.SeverityId, null, HighlightingGroupIds.CompilerWarnings,
        AnonymousObjectDestructuringWarning.Message, AnonymousObjectDestructuringWarning.Message,
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

        private readonly DocumentRange _documentRange;

        public AnonymousObjectDestructuringWarning(DocumentRange documentRange)
        {
            _documentRange = documentRange;
        }

        public string ErrorStripeToolTip => ToolTip;

        public string ToolTip => Message;

        public DocumentRange CalculateRange()
        {
            return _documentRange;
        }

        public bool IsValid()
        {
            return _documentRange.IsValid();
        }
    }
}