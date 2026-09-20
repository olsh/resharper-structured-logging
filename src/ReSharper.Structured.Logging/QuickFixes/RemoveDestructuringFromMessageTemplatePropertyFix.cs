using System;

using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.TextControl;
using JetBrains.Util;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Models;
using ReSharper.Structured.Logging.Services;

namespace ReSharper.Structured.Logging.QuickFixes
{
    [QuickFix]
    public class RemoveDestructuringFromMessageTemplatePropertyFix : QuickFixBase
    {
        private readonly MessageTemplateTokenInformation _tokenInformation;

        public RemoveDestructuringFromMessageTemplatePropertyFix([NotNull] RedundantDestructuringOperatorWarning error)
        {
            _tokenInformation = error.TokenInformation;
        }

        public override string Text => "Remove destructuring operator";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            // An interpolated template carries no string literal to rewrite
            return _tokenInformation.DocumentRange.IsValid() && _tokenInformation.StringLiteral != null;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            // The operator is the character right after the opening brace of the hole
            var operatorIndex = _tokenInformation.RelativeStartIndex + 1;
            MessageTemplateLiteralRewriter.Rewrite(_tokenInformation, text => text.Remove(operatorIndex, 1));

            return null;
        }
    }
}
