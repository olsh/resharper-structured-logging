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
    public class AddDestructuringToMessageTemplatePropertyFix : QuickFixBase
    {
        private readonly MessageTemplateTokenInformation _tokenInformation;

        public AddDestructuringToMessageTemplatePropertyFix([NotNull] AnonymousObjectDestructuringWarning error)
        {
            _tokenInformation = error.TokenInformation;
        }

        public AddDestructuringToMessageTemplatePropertyFix([NotNull] ComplexObjectDestructuringWarning error)
        {
            _tokenInformation = error.TokenInformation;
        }

        public override string Text => "Add destructuring to property";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            // An interpolated template carries no string literal to rewrite
            return _tokenInformation.DocumentRange.IsValid() && _tokenInformation.StringLiteral != null;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            // The operator goes right after the opening brace of the hole
            var operatorIndex = _tokenInformation.RelativeStartIndex + 1;
            MessageTemplateLiteralRewriter.Rewrite(_tokenInformation, text => text.Insert(operatorIndex, "@"));

            return null;
        }
    }
}
