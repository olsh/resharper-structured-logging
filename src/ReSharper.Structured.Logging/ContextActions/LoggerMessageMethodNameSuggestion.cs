using System;
using System.Collections.Generic;
using System.Text;

using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

using ReSharper.Structured.Logging.Serilog.Events;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.ContextActions
{
    /// <summary>
    /// Derives the name of the generated method from the words of the message template, so that
    /// <c>"Order {OrderId} shipped to {Country}"</c> suggests <c>OrderShipped</c>.
    /// </summary>
    /// <remarks>
    /// The heuristic is deliberately blunt: it reads the literal text between the holes, drops the words that
    /// carry no meaning on their own, and keeps the first few. What makes that good enough is that the action
    /// leaves a rename hotspot on the name, so correcting it costs one keystroke.
    /// </remarks>
    public static class LoggerMessageMethodNameSuggestion
    {
        private const int MaximumWordCount = 4;

        /// <summary>
        /// Words that say nothing about the event on their own. Dropping them keeps
        /// <c>"Order shipped to {Country}"</c> from suggesting <c>OrderShippedTo</c>.
        /// </summary>
        private static readonly string[] StopWords =
        {
            "a", "an", "and", "as", "at", "by", "for", "from", "in", "into", "of", "on", "or", "the", "to",
            "with"
        };

        /// <summary>
        /// A PascalCase method name for the template, falling back to the level when the template is all
        /// holes and punctuation and no word survives.
        /// </summary>
        [NotNull]
        public static string Suggest([NotNull] MessageTemplate template, [NotNull] string fallbackName)
        {
            var words = CollectWords(template);
            if (words.Count == 0)
            {
                return fallbackName;
            }

            var name = new StringBuilder();
            foreach (var word in words)
            {
                name.Append(StringUtil.MakeUpperCamelCaseName(word));
            }

            var suggestion = name.ToString();

            // A template opening with a number, as in "404 returned for {Path}", cannot start an identifier
            return suggestion.IsNullOrEmpty() || !char.IsLetter(suggestion[0]) ? fallbackName : suggestion;
        }

        /// <summary>
        /// The suggestion with a numeric suffix when the target class already has a member of that name.
        /// A class that is about to be created has no declared element yet, and so nothing to collide with.
        /// </summary>
        [NotNull]
        public static string MakeUnique([NotNull] string name, [CanBeNull] IClassLikeDeclaration targetClass)
        {
            var typeElement = targetClass?.DeclaredElement;
            if (typeElement == null)
            {
                return name;
            }

            var takenNames = new JetHashSet<string>(StringComparer.Ordinal);
            foreach (var member in typeElement.GetMembers())
            {
                takenNames.Add(member.ShortName);
            }

            if (!takenNames.Contains(name))
            {
                return name;
            }

            var suffix = 2;
            while (takenNames.Contains(name + suffix))
            {
                suffix++;
            }

            return name + suffix;
        }

        /// <summary>
        /// The meaningful words of the literal text of the template, in order, capped so that a long sentence
        /// does not turn into an unreadable identifier.
        /// </summary>
        [NotNull]
        private static IReadOnlyList<string> CollectWords([NotNull] MessageTemplate template)
        {
            var words = new List<string>(MaximumWordCount);
            foreach (var token in template.Tokens)
            {
                if (!(token is TextToken textToken))
                {
                    continue;
                }

                foreach (var word in SplitWords(textToken.Text))
                {
                    if (StopWords.Contains(word, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    words.Add(word);
                    if (words.Count == MaximumWordCount)
                    {
                        return words;
                    }
                }
            }

            return words;
        }

        /// <summary>
        /// The runs of letters and digits in the text. Everything else, punctuation and whitespace alike, is
        /// a separator.
        /// </summary>
        [NotNull]
        private static IEnumerable<string> SplitWords([CanBeNull] string text)
        {
            if (text.IsNullOrEmpty())
            {
                yield break;
            }

            var word = new StringBuilder();
            foreach (var character in text)
            {
                if (char.IsLetterOrDigit(character))
                {
                    word.Append(character);

                    continue;
                }

                if (word.Length > 0)
                {
                    yield return word.ToString();
                    word.Clear();
                }
            }

            if (word.Length > 0)
            {
                yield return word.ToString();
            }
        }
    }
}
