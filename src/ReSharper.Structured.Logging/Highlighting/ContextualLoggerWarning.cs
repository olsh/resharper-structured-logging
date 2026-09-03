using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

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
    public class ContextualLoggerWarning : IHighlighting
    {
        private const string Message = "Incorrect type is used for contextual logger";

        public const string SeverityId = "ContextualLoggerProblem";

        private readonly DocumentRange _range;

        public ContextualLoggerWarning(
            DocumentRange documentRange,
            [CanBeNull] ITypeUsage typeArgument,
            [CanBeNull] ITypeElement expectedType,
            [CanBeNull] ICSharpParameterDeclaration parameterDeclaration = null)
        {
            _range = documentRange;
            TypeArgument = typeArgument;
            ExpectedType = expectedType;
            ParameterDeclaration = parameterDeclaration;
        }

        public string ErrorStripeToolTip => ToolTip;

        [CanBeNull]
        public ITypeElement ExpectedType { get; }

        /// <summary>
        /// The parameter the logger is declared on, or <c>null</c> when the warning comes from a factory
        /// call such as <c>ForContext&lt;T&gt;()</c> or <c>CreateLogger&lt;T&gt;()</c>.
        /// </summary>
        [CanBeNull]
        public ICSharpParameterDeclaration ParameterDeclaration { get; }

        public string ToolTip => Message;

        [CanBeNull]
        public ITypeUsage TypeArgument { get; }

        public DocumentRange CalculateRange()
        {
            return _range;
        }

        public bool IsValid()
        {
            return _range.IsValid();
        }
    }
}
