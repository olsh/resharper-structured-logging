using System;
using System.Linq;

using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Models;
using ReSharper.Structured.Logging.Serilog.Parsing;
using ReSharper.Structured.Logging.Services;

namespace ReSharper.Structured.Logging.QuickFixes
{
    /// <summary>
    /// Renames a template hole to a name derived from the argument that fills it.
    /// </summary>
    public abstract class RenameTemplatePropertyFixBase : QuickFixBase
    {
        [CanBeNull] private readonly ICSharpArgument _argument;

        private readonly PropertyToken _namedProperty;

        private readonly MessageTemplateTokenInformation _tokenInformation;

        private readonly string[] _usedPropertyNames;

        [CanBeNull] private string _suggestedName;

        private bool _suggestedNameCalculated;

        protected RenameTemplatePropertyFixBase([NotNull] PositionalPropertyUsedWarning error)
        {
            _tokenInformation = error.TokenInformation;
            _namedProperty = error.NamedProperty;
            _argument = error.Argument;
            _usedPropertyNames = error.UsedPropertyNames;
        }

        protected RenameTemplatePropertyFixBase([NotNull] DuplicateTemplatePropertyWarning error)
        {
            _tokenInformation = error.TokenInformation;
            _namedProperty = error.NamedProperty;
            _argument = error.Argument;
            _usedPropertyNames = error.UsedPropertyNames;
        }

        public override string Text => $"Rename property to '{SuggestedName}'";

        [CanBeNull]
        private string SuggestedName
        {
            get
            {
                if (!_suggestedNameCalculated)
                {
                    _suggestedName = CalculateSuggestedName();
                    _suggestedNameCalculated = true;
                }

                return _suggestedName;
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _tokenInformation.DocumentRange.IsValid()
                   && _tokenInformation.StringLiteral != null
                   && SuggestedName != null;
        }

        /// <summary>
        /// Returns the name this fix renames the hole to, or <c>null</c> when it has nothing to offer.
        /// </summary>
        [CanBeNull]
        protected abstract string GetSuggestedName([CanBeNull] string leafName, [CanBeNull] string qualifiedName);

        /// <summary>
        /// Appends a counter until the name no longer collides with another hole of the same template.
        /// </summary>
        [NotNull]
        protected string MakeUnique([NotNull] string name)
        {
            var candidate = name;
            var counter = 2;
            while (_usedPropertyNames.Contains(candidate, StringComparer.Ordinal))
            {
                candidate = name + counter++;
            }

            return candidate;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var suggestedName = SuggestedName;
            if (suggestedName == null)
            {
                return null;
            }

            using (WriteLockCookie.Create())
            {
                var literalExpression = _tokenInformation.StringLiteral.Expression;
                var factory = CSharpElementFactory.GetInstance(literalExpression, false);

                // A hole is the brace, an optional destructuring hint, the name, and then any alignment
                // and format, so replacing exactly the name keeps the rest of the hole intact
                var startIndex = _tokenInformation.RelativeStartIndex
                                 + (_namedProperty.Destructuring == Destructuring.Default ? 1 : 2);
                var templateText = literalExpression.GetUnquotedText()
                    .Remove(startIndex, _namedProperty.PropertyName.Length)
                    .Insert(startIndex, suggestedName);

                ModificationUtil.ReplaceChild(literalExpression, factory.CreateExpression($"\"{templateText}\""));
            }

            return null;
        }

        [CanBeNull]
        private string CalculateSuggestedName()
        {
            var (leafName, qualifiedName) = TemplatePropertyNameSuggestion.GetSuggestedNames(_argument?.Value);
            var suggestedName = GetSuggestedName(leafName, qualifiedName);

            return suggestedName == null ? null : MakeUnique(suggestedName);
        }
    }
}
