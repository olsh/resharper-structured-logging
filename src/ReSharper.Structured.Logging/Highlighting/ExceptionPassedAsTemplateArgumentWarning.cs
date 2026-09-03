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

        // The dedicated argument is taken, so the advice above does not apply: the only way out is to stop
        // passing this exception as a property
        private const string ExceptionArgumentOccupiedMessage =
            "Exception should not be passed as a template argument, the exception argument is already used";

        private readonly DocumentRange _documentRange;

        public ExceptionPassedAsTemplateArgumentWarning(
            [NotNull] ICSharpArgument exceptionArgument,
            [NotNull] ICSharpArgument templateArgument,
            [NotNull] IInvocationExpression invocationExpression,
            [CanBeNull] MessageTemplateTokenInformation tokenInformation,
            [CanBeNull] PropertyToken namedProperty,
            bool exceptionArgumentOccupied)
        {
            ExceptionArgument = exceptionArgument;
            TemplateArgument = templateArgument;
            InvocationExpression = invocationExpression;
            TokenInformation = tokenInformation;
            NamedProperty = namedProperty;
            ExceptionArgumentOccupied = exceptionArgumentOccupied;
            _documentRange = exceptionArgument.GetDocumentRange();
        }

        /// <summary>
        /// Whether another exception already fills the dedicated exception argument. The message is still wrong,
        /// but moving this one there would pass two exceptions, so no fix is offered.
        /// </summary>
        public bool ExceptionArgumentOccupied { get; }

        [NotNull]
        public ICSharpArgument ExceptionArgument { get; }

        [NotNull]
        public ICSharpArgument TemplateArgument { get; }

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

        public string ToolTip => ExceptionArgumentOccupied ? ExceptionArgumentOccupiedMessage : Message;

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
