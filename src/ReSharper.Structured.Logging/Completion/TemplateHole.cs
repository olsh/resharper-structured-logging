using System;
using System.Collections.Generic;

using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.Tree;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Completion;

/// <summary>
/// The template hole the caret sits in, together with everything a completion provider needs to fill
/// it: which logging call owns the template, which hole of it is being typed, and what text range the
/// completed name replaces.
/// </summary>
internal sealed class TemplateHole
{
    [NotNull] private readonly IInvocationExpression _invocation;

    [NotNull] private readonly ICSharpArgument _templateArgument;

    private readonly int _holesBefore;

    private readonly int _holesAfter;

    public TemplateHole(
        [NotNull] IInvocationExpression invocation,
        [NotNull] ICSharpArgument templateArgument,
        [NotNull] TemplateHolePosition position,
        int holesBefore,
        int holesAfter,
        [NotNull] ISet<string> usedPropertyNames)
    {
        _invocation = invocation;
        _templateArgument = templateArgument;
        _holesBefore = holesBefore;
        _holesAfter = holesAfter;
        Position = position;
        UsedPropertyNames = usedPropertyNames;
    }

    [NotNull]
    public TemplateHolePosition Position { get; }

    /// <summary>
    /// The names the template already binds, excluding the hole being typed, so that a name is not
    /// offered twice for one template.
    /// </summary>
    [NotNull]
    public ISet<string> UsedPropertyNames { get; }

    /// <summary>
    /// The arguments whose names can fill this hole, the one it binds to first, or <c>null</c> when the
    /// hole values cannot be tied to expressions at all. The holes before this one claim the first
    /// arguments and the ones after it claim the last, so only what is left in between is a candidate.
    /// </summary>
    [CanBeNull]
    public IReadOnlyList<ICSharpArgument> GetCandidateArguments()
    {
        // The hole values are hidden when they were passed as one array instead of being expanded
        var holeArguments = _invocation.GetTemplateHoleArguments(_templateArgument);
        if (holeArguments == null)
        {
            return null;
        }

        var lastIndex = holeArguments.Count - _holesAfter;
        if (_holesBefore >= lastIndex)
        {
            return Array.Empty<ICSharpArgument>();
        }

        var candidates = new ICSharpArgument[lastIndex - _holesBefore];
        for (var index = 0; index < candidates.Length; index++)
        {
            candidates[index] = holeArguments[_holesBefore + index];
        }

        return candidates;
    }

    [CanBeNull]
    public static TemplateHole TryLocate(
        [CanBeNull] ITreeNode nodeInFile,
        DocumentOffset caretOffset,
        [NotNull] TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider,
        [NotNull] MessageTemplateParser messageTemplateParser)
    {
        var literal = TryGetTemplateLiteral(nodeInFile);
        if (literal == null)
        {
            return null;
        }

        var invocation = literal.GetContainingNode<IInvocationExpression>();
        var templateArgument = TryGetTemplateArgument(
            invocation,
            literal,
            templateParameterNameAttributeProvider);
        if (templateArgument == null)
        {
            return null;
        }

        var contentRange = TryGetContentRange(literal, caretOffset);
        if (contentRange == null)
        {
            return null;
        }

        var templateText = contentRange.Value.GetText();
        var caretIndex = caretOffset.Offset - contentRange.Value.StartOffset.Offset;

        var holeStartIndex = FindHoleStartIndex(templateText, caretIndex);
        if (holeStartIndex < 0)
        {
            return null;
        }

        var nameStartIndex = FindNameStartIndex(templateText, holeStartIndex, caretIndex);
        if (nameStartIndex < 0)
        {
            return null;
        }

        var nameEndIndex = FindNameEndIndex(templateText, caretIndex);

        var holeEnd = FindHoleEnd(templateText, nameEndIndex);
        var holes = CountHoles(messageTemplateParser, templateText, holeStartIndex);

        return new TemplateHole(
            invocation,
            templateArgument,
            new TemplateHolePosition(
                new DocumentRange(
                    contentRange.Value.StartOffset.Shift(nameStartIndex),
                    contentRange.Value.StartOffset.Shift(nameEndIndex)),
                holeEnd.HasClosingBrace,
                holeEnd.SuffixEndIndex - nameEndIndex),
            holes.HolesBefore,
            holes.HolesAfter,
            holes.UsedPropertyNames);
    }

    [CanBeNull]
    private static ICSharpLiteralExpression TryGetTemplateLiteral([CanBeNull] ITreeNode nodeInFile)
    {
        // An interpolated string is not a literal expression, which is what keeps ZLogger 2.x out
        var literal = nodeInFile?.Parent as ICSharpLiteralExpression;
        if (literal?.Literal == null ||
            !literal.Literal.GetTokenType()
                .IsStringLiteral)
        {
            return null;
        }

        return literal;
    }

    /// <summary>
    /// The argument the literal is passed as, when it is the template of a logging call. A logging
    /// element is one the provider can name the template parameter of, which covers Serilog, NLog,
    /// Microsoft.Extensions.Logging, ZLogger 1.x and annotated wrappers alike.
    /// </summary>
    [CanBeNull]
    private static ICSharpArgument TryGetTemplateArgument(
        [CanBeNull] IInvocationExpression invocation,
        [NotNull] ICSharpLiteralExpression literal,
        [NotNull] TemplateParameterNameAttributeProvider templateParameterNameAttributeProvider)
    {
        // The holes of LoggerMessage.Define are filled by generic type arguments, which name nothing
        if (invocation == null || invocation.IsLoggerMessageDefineMethod())
        {
            return null;
        }

        var templateArgument = invocation.GetTemplateArgument(templateParameterNameAttributeProvider);

        // A concatenated template is not supported yet, and neither is a literal passed anywhere but
        // to the template parameter
        return ReferenceEquals(templateArgument?.Value, literal) ? templateArgument : null;
    }

    /// <summary>
    /// The document range of the literal contents, which skips the quotes, the verbatim <c>@</c> and the
    /// raw string delimiters alike, so the offsets inside it are the offsets the template parser reports.
    /// </summary>
    private static DocumentRange? TryGetContentRange(
        [NotNull] ICSharpLiteralExpression literal,
        DocumentOffset caretOffset)
    {
        var containingFile = literal.GetContainingFile();
        if (containingFile == null)
        {
            return null;
        }

        var contentRange = containingFile.GetDocumentRange(literal.GetStringLiteralContentTreeRange());

        return contentRange.IsValid() && contentRange.Contains(caretOffset)
            ? contentRange
            : (DocumentRange?)null;
    }

    /// <summary>
    /// Walks back from the caret to the brace that opened the hole it sits in, or returns -1 when there
    /// is none. A run of braces of even length is a run of escaped <c>{{</c> and opens nothing.
    /// </summary>
    private static int FindHoleStartIndex([NotNull] string templateText, int caretIndex)
    {
        var holeStartIndex = -1;
        for (var index = caretIndex - 1; index >= 0; index--)
        {
            if (templateText[index] == '}')
            {
                return -1;
            }

            if (templateText[index] != '{')
            {
                continue;
            }

            holeStartIndex = index;
            break;
        }

        if (holeStartIndex < 0)
        {
            return -1;
        }

        var runStartIndex = holeStartIndex;
        while (runStartIndex > 0 && templateText[runStartIndex - 1] == '{')
        {
            runStartIndex--;
        }

        return (holeStartIndex - runStartIndex + 1) % 2 == 0 ? -1 : holeStartIndex;
    }

    /// <summary>
    /// Where the name being typed starts, or -1 when nothing can be named at the caret.
    /// </summary>
    private static int FindNameStartIndex(
        [NotNull] string templateText,
        int holeStartIndex,
        int caretIndex)
    {
        var nameStartIndex = holeStartIndex + 1;

        // {@Name and {$Name name the same property, so both complete, and the operator is left alone
        if (nameStartIndex < caretIndex && IsDestructuringOperator(templateText[nameStartIndex]))
        {
            nameStartIndex++;
        }

        // Anything else between the brace and the caret is an alignment or a format, not a name
        for (var index = nameStartIndex; index < caretIndex; index++)
        {
            if (!IsValidInPropertyName(templateText[index]))
            {
                return -1;
            }
        }

        // A name cannot start with a digit, so a hole that does is a positional one, which the
        // positional properties analyzer and its rename fix are the answer to
        if (nameStartIndex < templateText.Length && char.IsDigit(templateText[nameStartIndex]))
        {
            return -1;
        }

        // The caret sits before the operator, so a name written here would land in front of it
        return caretIndex < templateText.Length && IsDestructuringOperator(templateText[caretIndex])
            ? -1
            : nameStartIndex;
    }

    private static int FindNameEndIndex([NotNull] string templateText, int caretIndex)
    {
        var nameEndIndex = caretIndex;
        while (nameEndIndex < templateText.Length && IsValidInPropertyName(templateText[nameEndIndex]))
        {
            nameEndIndex++;
        }

        return nameEndIndex;
    }

    /// <summary>
    /// Where the hole ends: how far past the name its alignment and format run, and whether a brace
    /// closes it there. A brace only closes the hole when everything between it and the name is a
    /// well formed alignment and format, so a brace further off in the message closes nothing, and
    /// neither does the first half of an escaped <c>}}</c> that some text away happens to reach.
    /// Message punctuation is left outside: a comma that begins no alignment and a colon that begins
    /// no format are text the hole has to be closed in front of.
    /// </summary>
    private static (int SuffixEndIndex, bool HasClosingBrace) FindHoleEnd(
        [NotNull] string templateText,
        int nameEndIndex)
    {
        if (nameEndIndex >= templateText.Length)
        {
            return (nameEndIndex, false);
        }

        if (templateText[nameEndIndex] == '}')
        {
            return (nameEndIndex, true);
        }

        var alignmentEndIndex = nameEndIndex;
        if (templateText[nameEndIndex] == ',')
        {
            alignmentEndIndex = FindAlignmentEndIndex(templateText, nameEndIndex);
            if (alignmentEndIndex == nameEndIndex)
            {
                return (nameEndIndex, false);
            }

            if (alignmentEndIndex < templateText.Length && templateText[alignmentEndIndex] == '}')
            {
                return (alignmentEndIndex, true);
            }
        }

        if (alignmentEndIndex >= templateText.Length || templateText[alignmentEndIndex] != ':')
        {
            return (alignmentEndIndex, false);
        }

        // A format runs to the brace and may hold a space on the way
        var formatEndIndex = alignmentEndIndex + 1;
        while (formatEndIndex < templateText.Length && IsValidInsideHole(templateText[formatEndIndex]))
        {
            formatEndIndex++;
        }

        if (formatEndIndex < templateText.Length && templateText[formatEndIndex] == '}')
        {
            return (formatEndIndex, true);
        }

        // With no brace to run to there is nothing to tell the format from the words after it, so it
        // ends at the first space: a format may hold one, but so may the message
        var unterminatedEndIndex = alignmentEndIndex + 1;
        while (unterminatedEndIndex < templateText.Length &&
               templateText[unterminatedEndIndex] != ' ' &&
               IsValidInsideHole(templateText[unterminatedEndIndex]))
        {
            unterminatedEndIndex++;
        }

        return (unterminatedEndIndex == alignmentEndIndex + 1 ? alignmentEndIndex : unterminatedEndIndex,
            false);
    }

    /// <summary>
    /// Where the alignment after the comma ends, or the index of the comma itself when what follows it
    /// is not one. An alignment is an optional minus and a run of digits.
    /// </summary>
    private static int FindAlignmentEndIndex([NotNull] string templateText, int commaIndex)
    {
        var alignmentEndIndex = commaIndex + 1;
        if (alignmentEndIndex < templateText.Length && templateText[alignmentEndIndex] == '-')
        {
            alignmentEndIndex++;
        }

        var digitsStartIndex = alignmentEndIndex;
        while (alignmentEndIndex < templateText.Length && char.IsDigit(templateText[alignmentEndIndex]))
        {
            alignmentEndIndex++;
        }

        return alignmentEndIndex == digitsStartIndex ? commaIndex : alignmentEndIndex;
    }

    /// <summary>
    /// Counts the holes on either side of the one being typed and collects the names they bind. The hole
    /// being typed claims no argument of its own yet, and re-completing a closed one has to keep offering
    /// the name it already carries, so its brace is taken out of the way before parsing. An unterminated
    /// hole would otherwise run into the hole after it and swallow it: a brace is valid inside a format,
    /// so the parser reads all of it as one stretch of text and the holes beyond go uncounted.
    /// </summary>
    private static (int HolesBefore, int HolesAfter, ISet<string> UsedPropertyNames) CountHoles(
        [NotNull] MessageTemplateParser messageTemplateParser,
        [NotNull] string templateText,
        int holeStartIndex)
    {
        var holesBefore = 0;
        var holesAfter = 0;
        var usedPropertyNames = new HashSet<string>(StringComparer.Ordinal);

        // Replacing the brace rather than removing it keeps every other index where the caller left it
        var withoutHole = templateText.Remove(holeStartIndex, 1)
            .Insert(holeStartIndex, " ");

        foreach (var token in messageTemplateParser.Parse(withoutHole)
                     .Tokens)
        {
            if (!(token is PropertyToken propertyToken))
            {
                continue;
            }

            if (propertyToken.StartIndex < holeStartIndex)
            {
                holesBefore++;
            }
            else
            {
                holesAfter++;
            }

            usedPropertyNames.Add(propertyToken.PropertyName);
        }

        return (holesBefore, holesAfter, usedPropertyNames);
    }

    /// <summary>
    /// The characters the template parser reads as part of a hole, minus the braces that open and close
    /// one. It is a wide set: the alignment and the format between them take nearly anything.
    /// </summary>
    private static bool IsValidInsideHole(char c)
    {
        return c != '{' &&
               c != '}' &&
               (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || c == ' ' || c == '+');
    }

    private static bool IsDestructuringOperator(char c)
    {
        return c == '@' || c == '$';
    }

    /// <summary>
    /// The characters a suggested name can be made of. The template parser also accepts a space in a
    /// property name, which is left out here so that the completed name does not swallow the text
    /// that follows the caret.
    /// </summary>
    private static bool IsValidInPropertyName(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '.';
    }
}
