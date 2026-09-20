using System;
using System.Collections.Generic;
using System.Globalization;

using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Impl;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Features.Intellisense.CodeCompletion.CSharp.Rules;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Serilog.Parsing;
using ReSharper.Structured.Logging.Services;

namespace ReSharper.Structured.Logging.Completion;

/// <summary>
/// Completes property names inside the <c>{...}</c> of a logging message template, naming them after the
/// arguments that fill the holes. Typing the hole is the moment the property gets its name, so the name
/// is offered there rather than only corrected afterwards by the naming analyzer and its quick fixes.
/// </summary>
/// <remarks>
/// A template declared with <c>[LoggerMessage]</c> is left alone: ReSharper completes it from the
/// parameters of the generated method and drops every lookup item it did not add itself, so there is
/// nothing to add there.
/// </remarks>
[Language(typeof(CSharpLanguage))]
public class MessageTemplatePropertyItemsProvider : CSharpItemsProviderBase<CSharpCodeCompletionContext>
{
    protected override bool IsAvailable(CSharpCodeCompletionContext context)
    {
        return context.BasicContext.CodeCompletionType == CodeCompletionType.BasicCompletion
               && TryLocateHole(context) != null;
    }

    protected override bool AddLookupItems(CSharpCodeCompletionContext context, IItemsCollector collector)
    {
        var hole = TryLocateHole(context);

        // The hole values are hidden when they were passed as one array instead of being expanded,
        // and there is nothing to derive a name from
        var holeArguments = hole?.Invocation.GetTemplateHoleArguments(hole.TemplateArgument);
        if (holeArguments == null || holeArguments.Count == 0)
        {
            return false;
        }

        // The holes before the caret claim the first arguments and the ones after it claim the last, so
        // only what is left in between can name this hole. The nth hole is filled by the nth argument,
        // which is why the window starts there and its names lead
        var firstArgumentIndex = hole.HolesBefore;
        var lastArgumentIndex = holeArguments.Count - hole.HolesAfter;
        if (firstArgumentIndex >= lastArgumentIndex)
        {
            return false;
        }

        var ranges = CodeCompletionContextProviderBase.GetTextLookupRanges(context.BasicContext, hole.NameRange);
        var offeredNames = new HashSet<string>(StringComparer.Ordinal);
        var order = 0;

        for (var argumentIndex = firstArgumentIndex; argumentIndex < lastArgumentIndex; argumentIndex++)
        {
            var (leafName, qualifiedName) = TemplatePropertyNameSuggestion.GetSuggestedNames(
                holeArguments[argumentIndex]
                    .Value);

            // The qualified name tells more about the value, so it leads: order.Id offers OrderId then Id
            foreach (var name in new[] { qualifiedName, leafName })
            {
                if (string.IsNullOrEmpty(name) ||
                    hole.UsedPropertyNames.Contains(name) ||
                    !offeredNames.Add(name))
                {
                    continue;
                }

                collector.Add(
                    new TemplatePropertyLookupItem(
                        name,
                        !hole.HasClosingBrace,
                        new LookupItemPlacement(
                            order.ToString("D4", CultureInfo.InvariantCulture),
                            argumentIndex == firstArgumentIndex
                                ? PlacementLocation.Top
                                : PlacementLocation.Generic))
                    {
                        Ranges = ranges
                    });
                order++;
            }
        }

        return order > 0;
    }

    /// <summary>
    /// Nothing but a property name can be written where the caret is, so the items of the other providers
    /// are dropped. They are kept when this one found nothing to offer, which leaves the usual list alone
    /// for a hole it cannot serve.
    /// </summary>
    protected override void TransformItems(CSharpCodeCompletionContext context, IItemsCollector collector)
    {
        foreach (var item in collector.Items)
        {
            if (item is TemplatePropertyLookupItem)
            {
                collector.RemoveWhere(lookupItem => !(lookupItem is TemplatePropertyLookupItem));

                return;
            }
        }
    }

    protected override LookupFocusBehaviour GetLookupFocusBehaviour(CSharpCodeCompletionContext context)
    {
        return context.BasicContext.Parameters.IsAutomaticCompletion
            ? LookupFocusBehaviour.Soft
            : LookupFocusBehaviour.Hard;
    }

    [CanBeNull]
    private static TemplateHole TryLocateHole([NotNull] CSharpCodeCompletionContext context)
    {
        var nodeInFile = context.NodeInFile;
        var solution = nodeInFile?.GetSolution();
        if (solution == null)
        {
            return null;
        }

        return TemplateHole.TryLocate(
            nodeInFile,
            context.BasicContext.EffectiveCaretDocumentOffset,
            nodeInFile.GetPsiServices()
                .GetCodeAnnotationsCache()
                .GetProvider<TemplateParameterNameAttributeProvider>(),
            solution.GetComponent<MessageTemplateParser>());
    }
}
