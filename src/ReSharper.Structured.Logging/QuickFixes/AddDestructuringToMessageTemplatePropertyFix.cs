using System;

using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Models;

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
            return _tokenInformation.DocumentRange.IsValid();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_tokenInformation.StringLiteral.Expression, false);
                var startIndex = _tokenInformation.RelativeStartIndex;
                var expression = factory.CreateExpression(
                    $"\"{_tokenInformation.StringLiteral.Expression.GetUnquotedText().Insert(startIndex + 1, "@")}\"");

                // ReSharper disable once AssignNullToNotNullAttribute
                ModificationUtil.ReplaceChild(_tokenInformation.StringLiteral.Expression, expression);
            }

            return null;
        }
    }
}
