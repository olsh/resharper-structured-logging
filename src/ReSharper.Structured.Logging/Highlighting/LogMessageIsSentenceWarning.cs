using System.Text.RegularExpressions;

using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

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
    public class LogMessageIsSentenceWarning : IHighlighting
    {
        private const string Message = "Log event messages should be fragments, not sentences. Avoid a trailing period/full stop.";

        public const string SeverityId = "LogMessageIsSentenceProblem";

        private readonly DocumentRange _documentRange;

        public LogMessageIsSentenceWarning(IStringLiteralAlterer stringLiteral, Regex regex)
            : this(stringLiteral.Expression.GetDocumentRange(), stringLiteral, regex)
        {
        }

        /// <summary>
        /// The overload for a template that is not a string literal, such as the interpolated string of a
        /// ZLogger 2.x call. <see cref="StringLiteral"/> stays <c>null</c>, which keeps
        /// <c>RemoveTrailingPeriodFix</c> unavailable rather than letting it rewrite the template as a
        /// plain quoted string.
        /// </summary>
        public LogMessageIsSentenceWarning(DocumentRange documentRange, Regex regex)
            : this(documentRange, null, regex)
        {
        }

        private LogMessageIsSentenceWarning(
            DocumentRange documentRange,
            [CanBeNull] IStringLiteralAlterer stringLiteral,
            Regex regex)
        {
            StringLiteral = stringLiteral;
            Regex = regex;
            _documentRange = documentRange;
        }

        public string ErrorStripeToolTip => ToolTip;

        public string ToolTip => Message;

        [CanBeNull]
        public IStringLiteralAlterer StringLiteral { get; }

        public Regex Regex { get; }

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
