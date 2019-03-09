using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;

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

        private readonly IExpression _expression;

        public TemplateFormatStringArgumentIsNotUsedWarning(IExpression expression)
        {
            _expression = expression;
        }

        public string ErrorStripeToolTip => ToolTip;

        public string ToolTip => Message;

        public DocumentRange CalculateRange()
        {
            return _expression.GetHighlightingRange();
        }

        public bool IsValid()
        {
            if (_expression != null)
            {
                return _expression.IsValid();
            }

            return true;
        }
    }
}
