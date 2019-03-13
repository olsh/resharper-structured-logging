using System;

using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.DocumentModel;
using JetBrains.DocumentModel.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.FeaturesTestFramework.Utils;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.ConstantValues;
using JetBrains.ReSharper.Psi.CSharp.Util.Literals;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.QuickFixes
{
    [QuickFix]
    public class AddDestructuringToMessageTemplatePropertyFix : QuickFixBase
    {
        private readonly DocumentRange _range;

        private readonly IStringLiteralAlterer _stringLiteral;

        private readonly PropertyToken _namedProperty;

        public AddDestructuringToMessageTemplatePropertyFix([NotNull] AnonymousObjectDestructuringWarning error)
        {
            _range = error.Range;
            _stringLiteral = error.StringLiteral;
            _namedProperty = error.NamedProperty;
        }

        public override string Text => "Add destructuring to property";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _range.IsValid();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_stringLiteral.Expression, false);
                var expression = factory.CreateExpression(
                    $"\"{_stringLiteral.Expression.GetUnquotedText().Insert(_namedProperty.StartIndex + 1, "@")}\"");
                ModificationUtil.ReplaceChild(_stringLiteral.Expression, expression);
            }

            return null;
        }
    }
}
