using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

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
    public class ComplexObjectDestructuringInContextWarning : ComplexObjectDestructuringWarningBase, IHighlighting
    {
        public const string SeverityId = "ComplexObjectInContextDestructuringProblem";

        private readonly IInvocationExpression _invocationExpression;

        public ComplexObjectDestructuringInContextWarning(IInvocationExpression invocationExpression)
        {
            _invocationExpression = invocationExpression;
        }

        public string ErrorStripeToolTip => ToolTip;

        public string ToolTip => Message;

        public DocumentRange CalculateRange()
        {
            return _invocationExpression.GetDocumentRange();
        }

        public bool IsValid()
        {
            return _invocationExpression.GetDocumentRange().IsValid();
        }
    }
}
