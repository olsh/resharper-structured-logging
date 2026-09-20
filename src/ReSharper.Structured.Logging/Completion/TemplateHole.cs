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
    public TemplateHole(
        [NotNull] IInvocationExpression invocation,
        [NotNull] ICSharpArgument templateArgument,
        int holesBefore,
        int holesAfter,
        DocumentRange nameRange,
        bool hasClosingBrace,
        int suffixLength,
        [NotNull] ISet<string> usedPropertyNames)
    {
        Invocation = invocation;
        TemplateArgument = templateArgument;
        HolesBefore = holesBefore;
        HolesAfter = holesAfter;
        NameRange = nameRange;
        HasClosingBrace = hasClosingBrace;
        SuffixLength = suffixLength;
        UsedPropertyNames = usedPropertyNames;
    }

    [NotNull]
    public IInvocationExpression Invocation { get; }

    [NotNull]
    public ICSharpArgument TemplateArgument { get; }

    /// <summary>
    /// The number of complete holes before this one. A hole still being typed is not a hole to the
    /// parser, so it never counts itself, and the count is also the index of the argument this hole
    /// binds to.
    /// </summary>
    public int HolesBefore { get; }

    /// <summary>
    /// The number of complete holes after this one. They claim the last arguments, so the hole being
    /// typed can only be named after an argument that comes before them.
    /// </summary>
    public int HolesAfter { get; }

    /// <summary>
    /// The range of the property name being typed, empty when the caret sits right after the brace.
    /// The destructuring operator, the alignment and the format are outside it and are kept as they are.
    /// </summary>
    public DocumentRange NameRange { get; }

    public bool HasClosingBrace { get; }

    /// <summary>
    /// How many characters of alignment and format sit between the name and the end of the hole. The
    /// closing brace belongs after them, not after the name.
    /// </summary>
    public int SuffixLength { get; }

    /// <summary>
    /// The names the template already binds, excluding the hole being typed, so that a name is not
    /// offered twice for one template.
    /// </summary>
    [NotNull]
    public ISet<string> UsedPropertyNames { get; }

    /// <summary>
    /// Returns the hole under the caret, or <c>null</c> when the caret is not inside the <c>{...}</c> of
    /// a logging call message template.
    /// </summary>
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
        var suffixEndIndex = FindSuffixEndIndex(templateText, nameEndIndex);
        var holes = CountHoles(messageTemplateParser, templateText, holeStartIndex);

        return new TemplateHole(
            invocation,
            templateArgument,
            holes.HolesBefore,
            holes.HolesAfter,
            new DocumentRange(
                contentRange.Value.StartOffset.Shift(nameStartIndex),
                contentRange.Value.StartOffset.Shift(nameEndIndex)),
            IsClosingBraceAhead(templateText, suffixEndIndex),
            suffixEndIndex - nameEndIndex,
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
    /// Where the alignment and format that follow the name end. They run to the end of the hole, which
    /// an unterminated hole does not have, so the run is cut at the first space: a format can hold one,
    /// but so can the words of the message, and swallowing those would be the worse mistake.
    /// </summary>
    private static int FindSuffixEndIndex([NotNull] string templateText, int nameEndIndex)
    {
        if (nameEndIndex >= templateText.Length ||
            (templateText[nameEndIndex] != ',' && templateText[nameEndIndex] != ':'))
        {
            return nameEndIndex;
        }

        var suffixEndIndex = nameEndIndex;
        while (suffixEndIndex < templateText.Length &&
               !char.IsWhiteSpace(templateText[suffixEndIndex]) &&
               templateText[suffixEndIndex] != '{' &&
               templateText[suffixEndIndex] != '}')
        {
            suffixEndIndex++;
        }

        return suffixEndIndex;
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

    private static bool IsClosingBraceAhead([NotNull] string templateText, int nameEndIndex)
    {
        for (var index = nameEndIndex; index < templateText.Length; index++)
        {
            if (templateText[index] == '}')
            {
                return true;
            }

            if (templateText[index] == '{')
            {
                return false;
            }
        }

        return false;
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
