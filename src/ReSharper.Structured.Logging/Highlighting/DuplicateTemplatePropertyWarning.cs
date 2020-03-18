using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Settings;

[assembly:
    RegisterConfigurableSeverity(
        DuplicateTemplatePropertyWarning.SeverityId,
        null,
        StructuredLoggingGroup.Id,
        DuplicateTemplatePropertyWarning.Message,
        DuplicateTemplatePropertyWarning.Message,
        Severity.WARNING)]

namespace ReSharper.Structured.Logging.Highlighting
{
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        OverlapResolve = OverlapResolveKind.WARNING,
        ToolTipFormatString = Message)]
    public class DuplicateTemplatePropertyWarning : IHighlighting
    {
        public const string Message = "Duplicate properties in message template";

        public const string SeverityId = "TemplateDuplicatePropertyProblem";

        private readonly DocumentRange _documentRange;

        public DuplicateTemplatePropertyWarning(DocumentRange documentRange)
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
