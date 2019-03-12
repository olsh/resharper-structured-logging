using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.Structured.Logging.Highlighting
{
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        AttributeId = HighlightingAttributeIds.DEADCODE_ATTRIBUTE,
        OverlapResolve = OverlapResolveKind.DEADCODE,
        ToolTipFormatString = Message)]
    public class TemplateFormatStringArgumentIsNotUsedWarning : IHighlighting
    {
        private const string Message = "Argument is not used in message template";

        private const string SeverityId = "TemplateFormatStringProblem";

        public TemplateFormatStringArgumentIsNotUsedWarning(ICSharpArgument argument)
        {
            Argument = argument;
        }

        public ICSharpArgument Argument { get; }

        public string ErrorStripeToolTip => ToolTip;

        public string ToolTip => Message;

        public DocumentRange CalculateRange()
        {
            return Argument.Expression.GetHighlightingRange();
        }

        public bool IsValid()
        {
            if (Argument.Expression != null)
            {
                return Argument.Expression.IsValid();
            }

            return true;
        }
    }
}
