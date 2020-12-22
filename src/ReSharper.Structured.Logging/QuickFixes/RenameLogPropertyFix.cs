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
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.QuickFixes
{
    [QuickFix]
    public class RenameLogPropertyFix : QuickFixBase
    {
        private readonly PropertyToken _namedProperty;

        private readonly MessageTemplateTokenInformation _tokenInformation;

        private readonly string _suggestedName;

        public RenameLogPropertyFix([NotNull] InconsistentLogPropertyNamingWarning error)
        {
            _namedProperty = error.NamedProperty;
            _suggestedName = error.SuggestedName;
            _tokenInformation = error.TokenInformation;
        }

        public override string Text => $"Rename property to '{_suggestedName}'";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _tokenInformation.DocumentRange.IsValid();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_tokenInformation.StringLiteral.Expression, false);
                var relativeStartIndex = _tokenInformation.RelativeStartIndex;
                var startIndex = _namedProperty.Destructuring == Destructuring.Default
                                     ? relativeStartIndex + 1
                                     : relativeStartIndex + 2;
                var length = _namedProperty.Destructuring == Destructuring.Default
                                 ? _namedProperty.Length - 2
                                 : _namedProperty.Length - 3;

                var expression = factory.CreateExpression(
                    $"\"{_tokenInformation.StringLiteral.Expression.GetUnquotedText().Remove(startIndex, length).Insert(startIndex, _suggestedName)}\"");
                ModificationUtil.ReplaceChild(_tokenInformation.StringLiteral.Expression, expression);
            }

            return null;
        }
    }
}
