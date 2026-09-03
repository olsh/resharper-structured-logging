using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi.CSharp.Tree;

using ReSharper.Structured.Logging.Models;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Highlighting
{
    /// <summary>
    /// The payload shared by the template property warnings a rename fix can act on.
    /// </summary>
    public abstract class TemplatePropertyWarningBase
    {
        protected TemplatePropertyWarningBase(
            [NotNull] MessageTemplateTokenInformation tokenInformation,
            [NotNull] PropertyToken namedProperty,
            [CanBeNull] ICSharpArgument argument,
            [NotNull] string[] usedPropertyNames)
        {
            TokenInformation = tokenInformation;
            NamedProperty = namedProperty;
            Argument = argument;
            UsedPropertyNames = usedPropertyNames;
        }

        [NotNull]
        public MessageTemplateTokenInformation TokenInformation { get; }

        [NotNull]
        public PropertyToken NamedProperty { get; }

        /// <summary>
        /// The argument that fills the hole, or <c>null</c> when there is none to derive a name from,
        /// as in an attribute template or when the values are passed as a single array.
        /// </summary>
        [CanBeNull]
        public ICSharpArgument Argument { get; }

        /// <summary>
        /// Every property name the template already uses, so a fix can avoid renaming into a collision.
        /// </summary>
        [NotNull]
        public string[] UsedPropertyNames { get; }

        public DocumentRange CalculateRange()
        {
            return TokenInformation.DocumentRange;
        }

        public bool IsValid()
        {
            return TokenInformation.DocumentRange.IsValid();
        }
    }
}
