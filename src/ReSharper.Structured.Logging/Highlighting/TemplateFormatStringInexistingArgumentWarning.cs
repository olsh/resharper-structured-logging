using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;

using ReSharper.Structured.Logging.Highlighting;

[assembly:
    RegisterConfigurableSeverity(TemplateFormatStringInexistingArgumentWarning.SeverityId, null, HighlightingGroupIds.CompilerWarnings,
        TemplateFormatStringInexistingArgumentWarning.Message, TemplateFormatStringInexistingArgumentWarning.Message,
        Severity.WARNING)]

namespace ReSharper.Structured.Logging.Highlighting
{
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        OverlapResolve = OverlapResolveKind.WARNING,
        ToolTipFormatString = Message)]
    public class TemplateFormatStringInexistingArgumentWarning : IHighlighting
    {
        public const string SeverityId = "TemplateFormatStringProblem";

        public const string Message = "Non-existing argument in message template";

        private readonly DocumentRange _documentRange;

        public TemplateFormatStringInexistingArgumentWarning(DocumentRange documentRange)
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
