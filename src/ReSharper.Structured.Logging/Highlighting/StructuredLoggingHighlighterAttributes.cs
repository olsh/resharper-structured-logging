using JetBrains.ReSharper.Feature.Services.Daemon.Attributes.Idea;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.TextControl.DocumentMarkup.VisualStudio;

namespace ReSharper.Structured.Logging.Highlighting
{
    /// <summary>
    /// The dimmed appearance mirrors how ReSharper renders dead code: a semi-transparent foreground on the
    /// dead code layer, which paints over syntax colors but still lets analysis squiggles show through.
    /// </summary>
    [RegisterHighlighter(
        DimmedLoggingStatement,
        VsPresentableName = "ReSharper Structured Logging Dimmed Statement",
        RiderReplaceWith = IdeaHighlightingAttributeIds.NOT_USED_ELEMENT_ATTRIBUTES,
        Layer = HighlighterLayer.DEADCODE,
        EffectType = EffectType.TEXT,
        ForegroundOpacity = 0.5,
        VsGenerateClassificationDefinition = VsGenerateDefinition.VisibleClassification)]
    public static class StructuredLoggingHighlighterAttributes
    {
        public const string DimmedLoggingStatement = "ReSharper Structured Logging Dimmed Statement";
    }
}
