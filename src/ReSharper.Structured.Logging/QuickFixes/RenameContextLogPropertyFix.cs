using System;

using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.QuickFixes
{
    [QuickFix]
    public class RenameContextLogPropertyFix : QuickFixBase
    {
        private readonly ICSharpArgument _argument;

        private readonly string _suggestedName;

        public RenameContextLogPropertyFix([NotNull] InconsistentContextLogPropertyNamingWarning error)
        {
            _suggestedName = error.SuggestedName;
            _argument = error.Argument;
        }

        public override string Text => $"Rename property to '{_suggestedName}'";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _argument.IsValid();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var factory = CSharpElementFactory.GetInstance(_argument.Expression, false);

                var expression = factory.CreateExpression($"\"{_suggestedName}\"");
                ModificationUtil.ReplaceChild(_argument.Expression, expression);
            }

            return null;
        }
    }
}
