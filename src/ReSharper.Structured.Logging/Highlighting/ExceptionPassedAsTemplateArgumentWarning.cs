using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

using ReSharper.Structured.Logging.Models;
using ReSharper.Structured.Logging.Serilog.Parsing;
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
    public class ExceptionPassedAsTemplateArgumentWarning : IHighlighting
    {
        public const string SeverityId = "ExceptionPassedAsTemplateArgumentProblem";

        private const string Message = "Exception should be passed to the exception argument";

        private readonly DocumentRange _documentRange;

        public ExceptionPassedAsTemplateArgumentWarning(
            [NotNull] ICSharpArgument exceptionArgument,
            [NotNull] IInvocationExpression invocationExpression,
            [CanBeNull] MessageTemplateTokenInformation tokenInformation,
            [CanBeNull] PropertyToken namedProperty)
        {
            ExceptionArgument = exceptionArgument;
            InvocationExpression = invocationExpression;
            TokenInformation = tokenInformation;
            NamedProperty = namedProperty;
            _documentRange = exceptionArgument.GetDocumentRange();
        }

        [NotNull]
        public ICSharpArgument ExceptionArgument { get; }

        [NotNull]
        public IInvocationExpression InvocationExpression { get; }

        /// <summary>
        /// The template hole the exception is bound to, or <c>null</c> when the template is not a literal
        /// the fix can rewrite. The fix then only moves the argument.
        /// </summary>
        [CanBeNull]
        public MessageTemplateTokenInformation TokenInformation { get; }

        [CanBeNull]
        public PropertyToken NamedProperty { get; }

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
