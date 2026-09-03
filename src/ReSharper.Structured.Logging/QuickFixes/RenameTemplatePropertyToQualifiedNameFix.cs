using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.QuickFixes;

using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.QuickFixes
{
    /// <summary>
    /// Renames the hole to the qualified name of the argument that fills it, so a hole filled by
    /// <c>order.Customer.Name</c> becomes <c>{CustomerName}</c>. Offered next to
    /// <see cref="RenameTemplatePropertyFromArgumentFix"/> only when the two names differ.
    /// </summary>
    [QuickFix]
    public class RenameTemplatePropertyToQualifiedNameFix : RenameTemplatePropertyFixBase
    {
        public RenameTemplatePropertyToQualifiedNameFix([NotNull] PositionalPropertyUsedWarning error)
            : base(error)
        {
        }

        public RenameTemplatePropertyToQualifiedNameFix([NotNull] DuplicateTemplatePropertyWarning error)
            : base(error)
        {
        }

        protected override string GetSuggestedName(string leafName, string qualifiedName)
        {
            return qualifiedName;
        }
    }
}
