using System.Linq;

using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Intentions.CSharp.QuickFixes;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.QuickFixes
{
    [QuickFix]
    public class RemoveRedundantMessageTemplateArgumentValueFix : CSharpScopedRemoveRedundantCodeQuickFixBase
    {
        [NotNull]
        private readonly ICSharpArgument _argument;

        public RemoveRedundantMessageTemplateArgumentValueFix([NotNull] TemplateFormatStringArgumentIsNotUsedWarning error)
        {
            _argument = error.Argument;
        }

        public override bool IsReanalysisRequired => false;

        public override string ScopedText => "Remove redundant argument(s) values";

        public override string Text => "Remove redundant argument(s) value";

        public override ITreeNode ReanalysisDependencyRoot => _argument;

        public override void Execute()
        {
            var containingNode = _argument.GetContainingNode<ICSharpArgumentsOwner>(true);
            if (containingNode == null)
            {
                return;
            }

            foreach (var csharpArgument in containingNode.Arguments.SkipWhile(a => a != _argument)
                .ToList())
            {
                containingNode.RemoveArgument(csharpArgument);
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _argument.IsValid();
        }

        protected override ITreeNode TryGetContextTreeNode()
        {
            return _argument;
        }
    }
}
