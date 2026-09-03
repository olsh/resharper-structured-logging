using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.QuickFixes;

using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.QuickFixes
{
    /// <summary>
    /// Renames the hole to the short name of the argument that fills it, so a hole filled by
    /// <c>order.Customer.Name</c> becomes <c>{Name}</c>.
    /// </summary>
    [QuickFix]
    public class RenameTemplatePropertyFromArgumentFix : RenameTemplatePropertyFixBase
    {
        private readonly bool _hasFallbackName;

        private readonly string _propertyName;

        public RenameTemplatePropertyFromArgumentFix([NotNull] PositionalPropertyUsedWarning error)
            : base(error)
        {
            _propertyName = error.NamedProperty.PropertyName;
        }

        public RenameTemplatePropertyFromArgumentFix([NotNull] DuplicateTemplatePropertyWarning error)
            : base(error)
        {
            // A duplicate already carries a name, so it can always fall back to a numbered variant of itself
            _propertyName = error.NamedProperty.PropertyName;
            _hasFallbackName = true;
        }

        protected override string GetSuggestedName(string leafName, string qualifiedName)
        {
            if (leafName != null)
            {
                return leafName;
            }

            // A positional hole has no name worth keeping, so with no argument there is nothing to suggest
            return _hasFallbackName ? _propertyName : null;
        }
    }
}
