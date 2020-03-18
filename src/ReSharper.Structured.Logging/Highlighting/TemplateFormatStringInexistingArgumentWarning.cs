using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Settings;

[assembly:
    RegisterConfigurableSeverity(
        TemplateFormatStringUnexistingArgumentWarning.SeverityId,
        null,
        StructuredLoggingGroup.Id,
        TemplateFormatStringUnexistingArgumentWarning.Message,
        TemplateFormatStringUnexistingArgumentWarning.Message,
        Severity.WARNING)]

namespace ReSharper.Structured.Logging.Highlighting
{
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        OverlapResolve = OverlapResolveKind.WARNING,
        ToolTipFormatString = Message)]
    public class TemplateFormatStringUnexistingArgumentWarning : IHighlighting
    {
        public const string SeverityId = "TemplateFormatStringProblem";

        public const string Message = "Non-existing argument in message template";

        private readonly DocumentRange _documentRange;

        public TemplateFormatStringUnexistingArgumentWarning(DocumentRange documentRange)
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
