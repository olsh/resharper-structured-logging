using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Hotspots;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Naming;
using JetBrains.ReSharper.Psi.Naming.Elements;
using JetBrains.ReSharper.Psi.Naming.Extentions;
using JetBrains.ReSharper.Psi.Naming.Impl;
using JetBrains.ReSharper.Psi.Naming.Settings;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.ReSharper.TestRunner.Abstractions.Extensions;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.QuickFixes
{
    [QuickFix]
    public class TemplateIsNotCompileTimeConstantFix : QuickFixBase
    {
        public TemplateIsNotCompileTimeConstantFix([NotNull] TemplateIsNotCompileTimeConstantWarning error)
        {
            InvocationExpression = error.InvocationExpression;
            MessageTemplateArgument = error.MessageTemplateArgument;
        }

        public override string Text => "Convert to compile-time constant message template";
        public IInvocationExpression InvocationExpression { get; set; }
        public ICSharpArgument MessageTemplateArgument { get; set; }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return InvocationExpression.IsValid() && MessageTemplateArgument.Expression is IInterpolatedStringExpression;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var interpolatedExpression = (IInterpolatedStringExpression)MessageTemplateArgument.Expression.NotNull();
            var elementFactory = CSharpElementFactory.GetInstance(interpolatedExpression);
            var hotspots = new List<(int Start, int End, List<string> Suggestions)>();

            var namingManager = solution.GetPsiServices().Naming;
            var nameSuggestionManager = namingManager.Suggestion;
            var namingLanguageService = NamingManager.GetNamingLanguageService(interpolatedExpression.Language);
            var entryOptions = new EntryOptions(
                PluralityKinds.Unknown,
                SubrootPolicy.Decompose,
                PredefinedPrefixPolicy.Remove);
            var settingsStore = interpolatedExpression.GetSettingsStoreWithEditorConfig();
            var suggestionOptions = new SuggestionOptions();
            var sourceFile = interpolatedExpression.GetSourceFile()
                .NotNull("interpolatedExpression.GetSourceFile() != null");

            using (WriteLockCookie.Create())
            {
                var hotspotsRegistry = new HotspotsRegistry(interpolatedExpression.GetPsiServices());

                var builder = new StringBuilder();
                foreach (var treeNode in interpolatedExpression.Children())
                {
                    if (treeNode is ITokenNode token)
                    {
                        if (treeNode.GetTokenType() == CSharpTokenType.INTERPOLATED_STRING_REGULAR_START)
                            builder.Append(token.GetText()[2..]);
                        else if (treeNode.GetTokenType() == CSharpTokenType.INTERPOLATED_STRING_VERBATIM_START)
                            builder.Append(token.GetText()[3..]);
                        else if (treeNode.GetTokenType() == CSharpTokenType.INTERPOLATED_STRING_REGULAR_END ||
                                 treeNode.GetTokenType() == CSharpTokenType.INTERPOLATED_STRING_VERBATIM_END)
                            builder.Append(token.GetText()[..^1]);
                        else
                            builder.Append(token.GetText());
                        continue;
                    }

                    var insert = (IInterpolatedStringInsert)treeNode;

                    var namesCollection = nameSuggestionManager.CreateEmptyCollection(
                        PluralityKinds.Unknown,
                        treeNode.Language,
                        longerNamesFirst: true,
                        sourceFile);
                    var suggestRoots = namingLanguageService.SuggestRoots(
                        insert.Expression,
                        useExpectedTypes: false,
                        namesCollection.PolicyProvider);

                    foreach (var suggestRoot in suggestRoots)
                        namesCollection.Add(suggestRoot, entryOptions);

                    var defaultRule = namingManager.Policy.GetDefaultRule(
                        sourceFile,
                        interpolatedExpression.Language,
                        settingsStore,
                        NamedElementKinds.Property,
                        ElementKindOfElementType.PROPERTY);
                    var namesSuggestion = namesCollection.Prepare(defaultRule, ScopeKind.Common, suggestionOptions);

                    hotspots.Add((
                        builder.Length + 1,
                        builder.Length + 1 + namesSuggestion.FirstName().Length,
                        namesSuggestion.AllNames().ToList()));
                    builder.Append(namesSuggestion.FirstName());

                    var argument = elementFactory.CreateArgument(ParameterKind.VALUE, insert.Expression);
                    InvocationExpression.AddArgumentAfter(argument, InvocationExpression.Arguments.Last());
                }

                var literalExpression = elementFactory.CreateStringLiteralExpression(builder.ToString());
                var literalExpressionArgument = InvocationExpression.AddArgumentAfter(
                    elementFactory.CreateArgument(ParameterKind.VALUE, literalExpression),
                    MessageTemplateArgument);
                InvocationExpression.RemoveArgument(MessageTemplateArgument);

                foreach (var (start, end, suggestions) in hotspots)
                {
                    var documentRange = new DocumentRange(
                        literalExpressionArgument.GetDocumentStartOffset().Shift(start),
                        literalExpressionArgument.GetDocumentStartOffset().Shift(end));
                    hotspotsRegistry.Register(documentRange.CreateRangeMarker(), new TextHotspotExpression(suggestions));
                }

                var endSelectionRange = ExpressionStatementNavigator.GetByExpression(
                    InvocationExpressionNavigator.GetByArgument(literalExpressionArgument))
                    .GetDocumentEndOffset();
                return BulbActionUtils.ExecuteHotspotSession(hotspotsRegistry, endSelectionRange);
            }
        }
    }
}
