using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;

namespace ReSharper.Structured.Logging.Highlighting
{
    /// <summary>
    /// Purely decorative: it dims a logging statement so it stops competing with the surrounding code.
    /// Unlike the other highlightings in this folder it is not an inspection, so it has no configurable
    /// severity and no error stripe marker; it is toggled through <c>StructuredLoggingSettings</c> instead.
    /// </summary>
    [StaticSeverityHighlighting(
        Severity.INFO,
        typeof(DimmedLoggingStatementHighlighting.StructuredLoggingHighlightings),
        AttributeId = StructuredLoggingHighlighterAttributes.DimmedLoggingStatement,
        OverlapResolve = OverlapResolveKind.NONE,
        ShowToolTipInStatusBar = false)]
    public class DimmedLoggingStatementHighlighting : IHighlighting
    {
        private readonly DocumentRange _documentRange;

        public DimmedLoggingStatementHighlighting(DocumentRange documentRange)
        {
            _documentRange = documentRange;
        }

        public string ErrorStripeToolTip => null;

        public string ToolTip => null;

        public DocumentRange CalculateRange()
        {
            return _documentRange;
        }

        public bool IsValid()
        {
            return _documentRange.IsValid();
        }

        [RegisterStaticHighlightingsGroup("Structured Logging", false)]
        public static class StructuredLoggingHighlightings
        {
        }
    }
}
