using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

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
    public class InconsistentContextLogPropertyNamingWarning : InconsistentLogPropertyNamingWarningBase, IHighlighting
    {
        private readonly string _propertyName;

        public const string SeverityId = "InconsistentContextLogPropertyNaming";

        public InconsistentContextLogPropertyNamingWarning(ICSharpArgument argument, string propertyName, string suggestedName)
        {
            _propertyName = propertyName;
            Argument = argument;
            SuggestedName = suggestedName;
        }

        public string ErrorStripeToolTip => ToolTip;

        public ICSharpArgument Argument { get; }


        public string SuggestedName { get; }

        public string ToolTip => GetToolTipMessage(_propertyName, SuggestedName);

        public DocumentRange CalculateRange()
        {
            return Argument.GetDocumentRange();
        }

        public bool IsValid()
        {
            return Argument.GetDocumentRange().IsValid();
        }
    }
}
