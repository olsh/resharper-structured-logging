using System;

using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.QuickFixes
{
    [QuickFix]
    public class RenameLogPropertyFix : QuickFixBase
    {
        private readonly DocumentRange _range;

        private readonly IStringLiteralAlterer _stringLiteral;

        private readonly PropertyToken _namedProperty;

        private readonly string _suggestedName;

        public RenameLogPropertyFix([NotNull] InconsistentLogPropertyNamingWarning error)
        {
            _range = error.Range;
            _stringLiteral = error.StringLiteral;
            _namedProperty = error.NamedProperty;
            _suggestedName = error.SuggestedName;
        }

        public override string Text => $"Rename property to '{_suggestedName}'";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _range.IsValid();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_stringLiteral.Expression, false);
                var startIndex = _namedProperty.Destructuring == Destructuring.Default
                                     ? _namedProperty.StartIndex + 1
                                     : _namedProperty.StartIndex + 2;
                var length = _namedProperty.Destructuring == Destructuring.Default
                                 ? _namedProperty.Length - 2
                                 : _namedProperty.Length - 3;

                var expression = factory.CreateExpression(
                    $"\"{_stringLiteral.Expression.GetUnquotedText().Remove(startIndex, length).Insert(startIndex, _suggestedName)}\"");
                ModificationUtil.ReplaceChild(_stringLiteral.Expression, expression);
            }

            return null;
        }
    }
}
