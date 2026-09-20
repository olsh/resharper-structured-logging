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
    public class ExceptionLoggedAsTextWarning : IHighlighting
    {
        public const string SeverityId = "ExceptionLoggedAsTextProblem";

        private const string Message = "Exception should be passed to the exception argument instead of its text";

        private readonly DocumentRange _documentRange;

        public ExceptionLoggedAsTextWarning(
            [NotNull] ICSharpArgument exceptionTextArgument,
            [NotNull] ICSharpExpression exceptionExpression,
            [NotNull] ICSharpArgument templateArgument,
            [NotNull] IInvocationExpression invocationExpression,
            [CanBeNull] MessageTemplateTokenInformation tokenInformation,
            [CanBeNull] PropertyToken namedProperty)
        {
            ExceptionTextArgument = exceptionTextArgument;
            ExceptionExpression = exceptionExpression;
            TemplateArgument = templateArgument;
            InvocationExpression = invocationExpression;
            TokenInformation = tokenInformation;
            NamedProperty = namedProperty;
            _documentRange = exceptionTextArgument.GetDocumentRange();
        }

        /// <summary>
        /// The argument that logs a piece of the exception as text, such as <c>exception.Message</c>.
        /// </summary>
        [NotNull]
        public ICSharpArgument ExceptionTextArgument { get; }

        /// <summary>
        /// The exception the text was read from. It is what the fix passes to the exception argument,
        /// and it is not always a plain variable: the receiver of a chain such as
        /// <c>exception.GetBaseException().Message</c> is the whole <c>exception.GetBaseException()</c> call.
        /// </summary>
        [NotNull]
        public ICSharpExpression ExceptionExpression { get; }

        [NotNull]
        public ICSharpArgument TemplateArgument { get; }

        [NotNull]
        public IInvocationExpression InvocationExpression { get; }

        /// <summary>
        /// The template hole the text is bound to, or <c>null</c> when the template is not a literal
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
