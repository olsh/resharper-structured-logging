using System;
using System.Text.RegularExpressions;

using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.QuickFixes
{
    [QuickFix]
    public class RemoveTrailingPeriodFix : QuickFixBase
    {
        private readonly IStringLiteralAlterer _stringLiteral;

        private readonly Regex _regex;

        public RemoveTrailingPeriodFix([NotNull] LogMessageIsSentenceWarning error)
        {
            _stringLiteral = error.StringLiteral;
            _regex = error.Regex;
        }

        public override string Text => "Remove period";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _stringLiteral.Expression.GetDocumentRange().IsValid();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_stringLiteral.Expression, false);
                var expression = factory.CreateExpression(
                    $"\"{_regex.Replace(_stringLiteral.Expression.GetUnquotedText(), string.Empty)}\"");

                ModificationUtil.ReplaceChild(_stringLiteral.Expression, expression);
            }

            return null;
        }
    }
}
