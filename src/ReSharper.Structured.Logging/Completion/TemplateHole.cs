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
    private TemplateHole(
        [NotNull] IInvocationExpression invocation,
        [NotNull] ICSharpArgument templateArgument,
        int holesBefore,
        int holesAfter,
        DocumentRange nameRange,
        bool hasClosingBrace,
        [NotNull] ISet<string> usedPropertyNames)
    {
        Invocation = invocation;
        TemplateArgument = templateArgument;
        HolesBefore = holesBefore;
        HolesAfter = holesAfter;
        NameRange = nameRange;
        HasClosingBrace = hasClosingBrace;
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
    /// The names the template already binds, excluding the hole being typed, so that a name is not
    /// offered twice for one template.
    /// </summary>
    [NotNull]
    public ISet<string> UsedPropertyNames { get; }

    /// <summary>
    /// Returns the hole under the caret, or <c>null</c> when the caret is not inside the <c>{...}</c> of
    /// a logging call's message template.
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

        // A logging element is one the provider can name the template parameter of, which covers
        // Serilog, NLog, Microsoft.Extensions.Logging, ZLogger 1.x and annotated wrappers alike
        var invocation = literal.GetContainingNode<IInvocationExpression>();
        var templateArgument = invocation?.GetTemplateArgument(templateParameterNameAttributeProvider);
        if (templateArgument == null || !ReferenceEquals(templateArgument.Value, literal))
        {
            return null;
        }

        // The holes of LoggerMessage.Define are filled by generic type arguments, which name nothing
        if (invocation.IsLoggerMessageDefineMethod())
        {
            return null;
        }

        var containingFile = literal.GetContainingFile();
        if (containingFile == null)
        {
            return null;
        }

        // The content range skips the quotes, the verbatim @ and the raw string delimiters alike, so the
        // offsets inside it are the offsets the template parser reports
        var contentRange = containingFile.GetDocumentRange(literal.GetStringLiteralContentTreeRange());
        if (!contentRange.IsValid() || !contentRange.Contains(caretOffset))
        {
            return null;
        }

        var templateText = contentRange.GetText();
        var caretIndex = caretOffset.Offset - contentRange.StartOffset.Offset;

        var holeStartIndex = FindHoleStartIndex(templateText, caretIndex);
        if (holeStartIndex < 0)
        {
            return null;
        }

        var nameStartIndex = holeStartIndex + 1;

        // {@Name and {$Name name the same property, so both complete, and the operator is left alone
        if (nameStartIndex < caretIndex &&
            (templateText[nameStartIndex] == '@' || templateText[nameStartIndex] == '$'))
        {
            nameStartIndex++;
        }

        if (!IsPropertyName(templateText, nameStartIndex, caretIndex))
        {
            return null;
        }

        // A name cannot start with a digit, so a hole that does is a positional one, which the
        // positional properties analyzer and its rename fix are the answer to
        if (nameStartIndex < templateText.Length && char.IsDigit(templateText[nameStartIndex]))
        {
            return null;
        }

        // The caret sits before the destructuring operator, so a name written here would land in front of it
        if (caretIndex < templateText.Length &&
            (templateText[caretIndex] == '@' || templateText[caretIndex] == '$'))
        {
            return null;
        }

        var nameEndIndex = caretIndex;
        while (nameEndIndex < templateText.Length && IsValidInPropertyName(templateText[nameEndIndex]))
        {
            nameEndIndex++;
        }

        var holesBefore = 0;
        var holesAfter = 0;
        var usedPropertyNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var token in messageTemplateParser.Parse(templateText)
                     .Tokens)
        {
            // The hole being typed claims no argument of its own yet, and re-completing a closed one
            // has to keep offering the name it already carries, so it is left out of both counts
            if (!(token is PropertyToken propertyToken) || propertyToken.StartIndex == holeStartIndex)
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

        return new TemplateHole(
            invocation,
            templateArgument,
            holesBefore,
            holesAfter,
            new DocumentRange(
                contentRange.StartOffset.Shift(nameStartIndex),
                contentRange.StartOffset.Shift(nameEndIndex)),
            IsClosingBraceAhead(templateText, nameEndIndex),
            usedPropertyNames);
    }

    [CanBeNull]
    private static ICSharpLiteralExpression TryGetTemplateLiteral([CanBeNull] ITreeNode nodeInFile)
    {
        // An interpolated string is not a literal expression, which is what keeps ZLogger 2.x out
        var literal = nodeInFile?.Parent as ICSharpLiteralExpression;

        return literal?.Literal?.GetTokenType()
            .IsStringLiteral == true
            ? literal
            : null;
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

    private static bool IsPropertyName([NotNull] string templateText, int startIndex, int endIndex)
    {
        for (var index = startIndex; index < endIndex; index++)
        {
            if (!IsValidInPropertyName(templateText[index]))
            {
                return false;
            }
        }

        return true;
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
