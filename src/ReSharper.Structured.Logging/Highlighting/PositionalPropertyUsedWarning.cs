using JetBrains.Annotations;
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
    public class PositionalPropertyUsedWarning : TemplatePropertyWarningBase, IHighlighting
    {
        private const string Message = "Prefer named properties instead of positional ones";

        public const string SeverityId = "PositionalPropertyUsedProblem";

        public PositionalPropertyUsedWarning(
            [NotNull] MessageTemplateTokenInformation tokenInformation,
            [NotNull] PropertyToken namedProperty,
            [CanBeNull] ICSharpArgument argument,
            [NotNull] string[] usedPropertyNames)
            : base(tokenInformation, namedProperty, argument, usedPropertyNames)
        {
        }

        public string ErrorStripeToolTip => ToolTip;

        public string ToolTip => Message;
    }
}
