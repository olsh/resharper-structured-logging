using JetBrains.DocumentModel;

namespace ReSharper.Structured.Logging.Completion;

/// <summary>
/// Where a completed name goes inside the hole and what has to follow it. The three travel together:
/// the name replaces the range, and the closing brace, when the template is missing one, goes after
/// the alignment and format rather than straight after the name.
/// </summary>
public sealed class TemplateHolePosition
{
    public TemplateHolePosition(DocumentRange nameRange, bool hasClosingBrace, int suffixLength)
    {
        NameRange = nameRange;
        HasClosingBrace = hasClosingBrace;
        SuffixLength = suffixLength;
    }

    /// <summary>
    /// The range of the property name being typed, empty when the caret sits right after the brace.
    /// The destructuring operator, the alignment and the format are outside it and are kept as they are.
    /// </summary>
    public DocumentRange NameRange { get; }

    public bool HasClosingBrace { get; }

    /// <summary>
    /// How many characters of alignment and format sit between the name and the end of the hole.
    /// </summary>
    public int SuffixLength { get; }
}
