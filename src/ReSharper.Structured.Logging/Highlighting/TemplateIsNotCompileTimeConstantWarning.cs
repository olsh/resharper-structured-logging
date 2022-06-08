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
    public class TemplateIsNotCompileTimeConstantWarning : IHighlighting
    {
        public const string SeverityId = "TemplateIsNotCompileTimeConstantProblem";

        private const string Message = "Message template should be compile time constant";

        public TemplateIsNotCompileTimeConstantWarning(
            IInvocationExpression invocationExpression,
            ICSharpArgument messageTemplateArgument)
        {
            InvocationExpression = invocationExpression;
            MessageTemplateArgument = messageTemplateArgument;
        }

        public IInvocationExpression InvocationExpression { get; }
        public ICSharpArgument MessageTemplateArgument { get; }

        public string ErrorStripeToolTip => ToolTip;

        public string ToolTip => Message;

        public DocumentRange CalculateRange()
        {
            return MessageTemplateArgument.Expression.GetDocumentRange();
        }

        public bool IsValid()
        {
            return MessageTemplateArgument.IsValid();
        }
    }
}
