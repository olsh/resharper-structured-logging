using System;
using System.Linq;

using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Naming;
using JetBrains.ReSharper.Psi.Naming.Elements;
using JetBrains.ReSharper.Psi.Naming.Extentions;
using JetBrains.ReSharper.Psi.Naming.Impl;
using JetBrains.ReSharper.Psi.Naming.Settings;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.Structured.Logging.Services
{
    /// <summary>
    /// Derives template property names from the expression that fills the hole, so that
    /// <c>order.Customer.Name</c> can suggest both <c>Name</c> and <c>CustomerName</c>.
    /// </summary>
    public static class TemplatePropertyNameSuggestion
    {
        /// <summary>
        /// Returns the short name built from the expression and, when the naming engine offers a longer
        /// candidate, the qualified one. Both are already converted to the configured naming style.
        /// Either can be <c>null</c> when nothing can be derived, which is the case for an attribute
        /// template, where no expression fills the hole.
        /// </summary>
        public static (string LeafName, string QualifiedName) GetSuggestedNames([CanBeNull] ICSharpExpression expression)
        {
            var sourceFile = expression?.GetSourceFile();
            if (sourceFile == null)
            {
                return (null, null);
            }

            var namingManager = expression.GetPsiServices().Naming;
            var namingLanguageService = NamingManager.GetNamingLanguageService(expression.Language);
            var settingsStore = expression.GetSettingsStoreWithEditorConfig();
            var namesCollection = namingManager.Suggestion.CreateEmptyCollection(
                PluralityKinds.Unknown,
                expression.Language,
                longerNamesFirst: true,
                sourceFile);

            var entryOptions = new EntryOptions(
                PluralityKinds.Unknown,
                SubrootPolicy.Decompose,
                PredefinedPrefixPolicy.Remove);
            foreach (var suggestRoot in namingLanguageService.SuggestRoots(
                         expression,
                         useExpectedTypes: false,
                         namesCollection.PolicyProvider))
            {
                namesCollection.Add(suggestRoot, entryOptions);
            }

            var defaultRule = namingManager.Policy.GetDefaultRule(
                sourceFile,
                expression.Language,
                settingsStore,
                NamedElementKinds.Property,
                ElementKindOfElementType.PROPERTY);

            var names = namesCollection.Prepare(defaultRule, ScopeKind.Common, new SuggestionOptions())
                .AllNames()
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => PropertyNameProvider.GetSuggestedName(n, settingsStore))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (names.Length == 0)
            {
                return (null, null);
            }

            // The engine is asked for longer names first, so the shortest candidate is the leaf name
            // and the longest is the qualified one
            var leafName = names.OrderBy(n => n.Length).First();
            var qualifiedName = names.OrderByDescending(n => n.Length).First();

            return (leafName, string.Equals(leafName, qualifiedName, StringComparison.Ordinal) ? null : qualifiedName);
        }
    }
}
