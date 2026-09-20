using System;

using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;

using ReSharper.Structured.Logging.Models;

namespace ReSharper.Structured.Logging.Services
{
    /// <summary>
    /// Rewrites the string literal a template token was read from, which is how the quick fixes that edit a
    /// hole in place, such as adding or removing its destructuring operator, apply their change.
    /// </summary>
    public static class MessageTemplateLiteralRewriter
    {
        /// <summary>
        /// Replaces the literal with a new one built from its unquoted text passed through <paramref name="rewrite"/>.
        /// The token's <see cref="MessageTemplateTokenInformation.StringLiteral"/> must not be <c>null</c>.
        /// </summary>
        public static void Rewrite(
            [NotNull] MessageTemplateTokenInformation tokenInformation,
            [NotNull] Func<string, string> rewrite)
        {
            using (WriteLockCookie.Create())
            {
                // ReSharper disable once PossibleNullReferenceException
                var literalExpression = tokenInformation.StringLiteral.Expression;
                var factory = CSharpElementFactory.GetInstance(literalExpression, false);
                var templateText = rewrite(literalExpression.GetUnquotedText());

                ModificationUtil.ReplaceChild(literalExpression, factory.CreateExpression($"\"{templateText}\""));
            }
        }
    }
}
