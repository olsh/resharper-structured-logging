using System.Collections.Generic;

using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

using ReSharper.Structured.Logging.Serilog.Events;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Models
{
    /// <summary>
    /// The message template of a logging call or logging attribute, together with the expression it was
    /// recovered from. A template written as a string literal is parsed by <see cref="MessageTemplateParser"/>;
    /// a ZLogger 2.x template is an interpolated string whose holes are the C# interpolations themselves.
    /// </summary>
    public sealed class LogMessageTemplate
    {
        /// <summary>
        /// The start indices of the holes whose property name can be checked against the naming rules, or
        /// <c>null</c> when every hole can be, which is the case for every template written as a literal.
        /// </summary>
        [CanBeNull] private readonly ISet<int> _nameCheckableStartIndexes;

        public LogMessageTemplate(
            [NotNull] ICSharpExpression expression,
            [NotNull] MessageTemplate template,
            [CanBeNull] ISet<int> nameCheckableStartIndexes = null)
        {
            Expression = expression;
            Template = template;
            _nameCheckableStartIndexes = nameCheckableStartIndexes;
        }

        [NotNull]
        public ICSharpExpression Expression { get; }

        [NotNull]
        public MessageTemplate Template { get; }

        /// <summary>
        /// Reports whether the naming rules can be applied to a hole. A ZLogger 2.x hole with no
        /// <c>:@name</c> specifier is named after the source text of its expression, so
        /// <c>{user.Name}</c> and <c>{GetCount()}</c> produce property names no rename could fix.
        /// Such holes still take part in duplicate detection, they are only left out of naming.
        /// </summary>
        public bool CanCheckPropertyName([NotNull] PropertyToken token)
        {
            return _nameCheckableStartIndexes == null || _nameCheckableStartIndexes.Contains(token.StartIndex);
        }
    }
}
