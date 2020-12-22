using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

namespace ReSharper.Structured.Logging.Models
{
    public class MessageTemplateTokenInformation
    {
        public MessageTemplateTokenInformation(
            DocumentRange documentRange,
            IStringLiteralAlterer stringLiteral)
        {
            DocumentRange = documentRange;
            StringLiteral = stringLiteral;
        }

        public DocumentRange DocumentRange { get; }

        public IStringLiteralAlterer StringLiteral { get; }

        public int RelativeStartIndex => DocumentRange.StartOffset - StringLiteral.Expression.GetDocumentRange().StartOffset - 1;
    }
}
