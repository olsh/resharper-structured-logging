using System.Collections.Generic;

using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

using ReSharper.Structured.Logging.Models;
using ReSharper.Structured.Logging.Serilog.Events;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Services
{
    /// <summary>
    /// Recovers the message template of a ZLogger 2.x logging call from the interpolated string bound to its
    /// handler parameter.
    /// </summary>
    /// <remarks>
    /// ZLogger names a hole after the source text of its expression, through
    /// <c>[CallerArgumentExpression]</c>, unless the format specifier starts with <c>@</c>, in which case the
    /// name is the text between the <c>@</c> and the next <c>:</c> and the rest is the real format. So
    /// <c>$"{userName}"</c> logs the key <c>userName</c>, <c>$"{x:@Host}"</c> logs <c>Host</c> and
    /// <c>$"{x:@Host:000}"</c> logs <c>Host</c> formatted as <c>000</c>. Alignment does not affect the key.
    /// </remarks>
    public static class InterpolatedMessageTemplateBuilder
    {
        [CanBeNull]
        public static LogMessageTemplate TryBuild([NotNull] IInterpolatedStringExpression interpolatedString)
        {
            var templateStartOffset = interpolatedString.GetDocumentRange()
                .TextRange.StartOffset;
            var tokens = new List<MessageTemplateToken>();
            var nameCheckableStartIndexes = new HashSet<int>();

            foreach (var insert in interpolatedString.Inserts)
            {
                var hole = TryGetHole(insert);
                if (hole == null)
                {
                    continue;
                }

                var (name, nameStartOffset, nameIsCheckable) = hole.Value;
                var startIndex = nameStartOffset - templateStartOffset;

                // The token is the property name alone rather than the whole `{...}` hole, so that its
                // StartIndex and Length, which PropertyToken derives from the raw text, describe exactly the
                // text a warning has to underline. The Serilog parser instead keeps the braces in the raw text.
                tokens.Add(new PropertyToken(name, name, startIndex: startIndex));

                if (nameIsCheckable)
                {
                    nameCheckableStartIndexes.Add(startIndex);
                }
            }

            if (tokens.Count == 0)
            {
                return null;
            }

            // The text is informational only, nothing maps offsets back onto it
            var template = new MessageTemplate(interpolatedString.GetText(), tokens);

            return new LogMessageTemplate(interpolatedString, template, nameCheckableStartIndexes);
        }

        /// <summary>
        /// Returns the property name of a hole, the document offset the name starts at, and whether the
        /// naming rules can be applied to it. An implicit name is only checkable when the hole holds a plain
        /// identifier: for <c>{user.Name}</c> or <c>{GetCount()}</c> the key ZLogger emits is the expression
        /// text itself, which no rename could turn into a well-formed property name.
        /// </summary>
        private static (string Name, int NameStartOffset, bool NameIsCheckable)? TryGetHole(
            [NotNull] IInterpolatedStringInsert insert)
        {
            var formatSpecifier = insert.FormatSpecifier;
            var formatSpecifierText = formatSpecifier?.GetText();

            // The format specifier text keeps its leading colon, so an explicit name starts with ":@"
            if (formatSpecifierText != null && formatSpecifierText.StartsWith(":@"))
            {
                const int namePrefixLength = 2;
                var formatSeparatorIndex = formatSpecifierText.IndexOf(':', namePrefixLength);
                var nameEndIndex = formatSeparatorIndex < 0 ? formatSpecifierText.Length : formatSeparatorIndex;
                var explicitName = formatSpecifierText.Substring(namePrefixLength, nameEndIndex - namePrefixLength);

                return explicitName.Length == 0
                    ? null
                    : (explicitName,
                        formatSpecifier.GetDocumentRange()
                            .TextRange.StartOffset + namePrefixLength, true);
            }

            var expression = insert.Expression;
            var implicitName = expression?.GetText();
            if (string.IsNullOrEmpty(implicitName))
            {
                return null;
            }

            var isPlainIdentifier = expression is IReferenceExpression { QualifierExpression: null };

            return (implicitName,
                expression.GetDocumentRange()
                    .TextRange.StartOffset, isPlainIdentifier);
        }
    }
}
