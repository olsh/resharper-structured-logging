using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;

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
    public class RedundantDestructuringOperatorWarning : IHighlighting
    {
        public const string SeverityId = "RedundantDestructuringOperatorProblem";

        private const string Message = "Destructuring operator has no effect on a scalar value";

        public RedundantDestructuringOperatorWarning(
            [NotNull] MessageTemplateTokenInformation tokenInformation,
            [NotNull] PropertyToken namedProperty)
        {
            TokenInformation = tokenInformation;
            NamedProperty = namedProperty;
        }

        public string ErrorStripeToolTip => ToolTip;

        [NotNull]
        public MessageTemplateTokenInformation TokenInformation { get; }

        [NotNull]
        public PropertyToken NamedProperty { get; }

        public string ToolTip => Message;

        public DocumentRange CalculateRange()
        {
            return TokenInformation.DocumentRange;
        }

        public bool IsValid()
        {
            return TokenInformation.DocumentRange.IsValid();
        }
    }
}
