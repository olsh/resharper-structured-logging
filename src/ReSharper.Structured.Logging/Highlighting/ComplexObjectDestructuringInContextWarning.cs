using JetBrains.Annotations;
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

        public ComplexObjectDestructuringInContextWarning([NotNull] IInvocationExpression invocationExpression)
        {
            InvocationExpression = invocationExpression;
        }

        public string ErrorStripeToolTip => ToolTip;

        [NotNull] public IInvocationExpression InvocationExpression { get; }

        public string ToolTip => Message;

        public DocumentRange CalculateRange()
        {
            return InvocationExpression.GetDocumentRange();
        }

        public bool IsValid()
        {
            return InvocationExpression.GetDocumentRange().IsValid();
        }
    }
}
