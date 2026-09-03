using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

namespace ReSharper.Structured.Logging.Models
{
    public class MessageTemplateTokenInformation
    {
        public MessageTemplateTokenInformation(
            DocumentRange documentRange,
            [CanBeNull] IStringLiteralAlterer stringLiteral)
        {
            DocumentRange = documentRange;
            StringLiteral = stringLiteral;
        }

        public DocumentRange DocumentRange { get; }

        /// <summary>
        /// The literal the token was read from, or <c>null</c> when the template is not one, as for the
        /// interpolated string of a ZLogger 2.x call. Every fix that rewrites the template has to check
        /// this before offering itself, because there is nothing for it to rewrite.
        /// </summary>
        [CanBeNull]
        public IStringLiteralAlterer StringLiteral { get; }

        /// <summary>
        /// The offset of the token inside the unquoted literal. Only meaningful when
        /// <see cref="StringLiteral"/> is not <c>null</c>.
        /// </summary>
        public int RelativeStartIndex => DocumentRange.StartOffset - StringLiteral.Expression.GetDocumentRange().StartOffset - 1;
    }
}
