using System;

using JetBrains.Annotations;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Daemon.CaretDependentFeatures;
using JetBrains.ReSharper.Feature.Services.Contexts;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.DataContext;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Serilog.Events;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Highlighting
{
    [ContainsContextConsumer]
    public class TemplateFormatItemAndMatchingArgumentHighlighter : ContextHighlighterBase
    {
        private readonly MessageTemplateParser _messageTemplate;

        private TemplateFormatItemAndMatchingArgumentHighlighter(MessageTemplateParser messageTemplate)
        {
            _messageTemplate = messageTemplate;
        }

        [AsyncContextConsumer]
        public static Action ProcessContext(
            [NotNull] Lifetime lifetime,
            [NotNull] HighlightingProlongedLifetime prolongedLifetime,
            [NotNull] [ContextKey(typeof(ContextHighlighterPsiFileView.ContextKey))]
            IPsiDocumentRangeView psiDocumentRangeView,
            MessageTemplateParser messageTemplate)
        {
            return new TemplateFormatItemAndMatchingArgumentHighlighter(messageTemplate).GetDataProcessAction(
                prolongedLifetime,
                psiDocumentRangeView);
        }

        public override Action GetDataProcessAction(
            HighlightingProlongedLifetime prolongedLifetime,
            IPsiDocumentRangeView psiDocumentRangeView)
        {
            var consumer = new HighlightingsConsumer();
            CollectHighlightings(psiDocumentRangeView, consumer);
            return () =>
                {
                    foreach (var highlightInfo in consumer.HighlightInfos)
                    {
                        CaretDependentFeaturesUtil.HighlightForLifetime(prolongedLifetime.Lifetime, highlightInfo);
                    }
                };
        }

        protected override void CollectHighlightings(
            IPsiDocumentRangeView psiDocumentRangeView,
            HighlightingsConsumer consumer)
        {
            var psiView = psiDocumentRangeView.View<CSharpLanguage>();
            var selectedArgument = psiView.GetSelectedTreeNode<ICSharpArgument>();
            if (selectedArgument == null)
            {
                return;
            }

            var invocationExpression = psiView.GetSelectedTreeNode<IInvocationExpression>();
            var templateArgument = invocationExpression?.GetTemplateArgument();
            if (templateArgument == null)
            {
                return;
            }

            if (templateArgument.IndexOf() > selectedArgument.IndexOf())
            {
                return;
            }

            var templateString = StringLiteralAltererUtil.TryCreateStringLiteralByExpression(templateArgument.Value)
                ?.Expression.GetUnquotedText();
            if (templateString == null)
            {
                return;
            }

            var messageTemplate = _messageTemplate.Parse(templateString);
            if (selectedArgument == templateArgument)
            {
                HighlightByNamedPlaceholder(
                    consumer,
                    psiView,
                    templateArgument,
                    messageTemplate,
                    invocationExpression.ArgumentList.Arguments);
            }
            else
            {
                HighlightByArgument(consumer, selectedArgument, templateArgument, messageTemplate);
            }
        }

        private static (PropertyToken token, int index) GetSelectedToken(
            IPsiView psiView,
            ICSharpArgument templateArgument,
            PropertyToken[] properties)
        {
            var documentRange = templateArgument.GetDocumentRange();
            var selectedTreeRange = psiView.GetSelectedTreeRange(templateArgument);

            var startSelectedIndex = selectedTreeRange.StartOffset.Offset - documentRange.StartOffset.Offset;
            var endSelectedIndex = selectedTreeRange.EndOffset.Offset - documentRange.StartOffset.Offset - 1;
            var propertyIndex = 0;
            foreach (var property in properties)
            {
                if (property.StartIndex < startSelectedIndex
                    && property.StartIndex + property.Length >= endSelectedIndex)
                {
                    return (property, propertyIndex);
                }

                propertyIndex++;
            }

            return (null, -1);
        }

        private static void HighlightByArgument(
            HighlightingsConsumer consumer,
            ICSharpArgument selectedArgument,
            ICSharpArgument templateArgument,
            MessageTemplate messageTemplate)
        {
            var argumentIndex = selectedArgument.IndexOf() - templateArgument.IndexOf() - 1;
            var namedProperties = messageTemplate.NamedProperties;
            var positionalProperties = messageTemplate.PositionalProperties;
            if (namedProperties != null && argumentIndex < namedProperties.Length)
            {
                var property = namedProperties[argumentIndex];
                consumer.ConsumeHighlighting(
                    HighlightingAttributeIds.USAGE_OF_ELEMENT_UNDER_CURSOR,
                    templateArgument.GetTokenDocumentRange(property));
                consumer.ConsumeHighlighting(
                    HighlightingAttributeIds.USAGE_OF_ELEMENT_UNDER_CURSOR,
                    selectedArgument.GetDocumentRange());
            }
            else if (positionalProperties != null)
            {
                foreach (var property in positionalProperties)
                {
                    if (!property.TryGetPositionalValue(out int position))
                    {
                        continue;
                    }

                    if (position != argumentIndex)
                    {
                        continue;
                    }

                    consumer.ConsumeHighlighting(
                        HighlightingAttributeIds.USAGE_OF_ELEMENT_UNDER_CURSOR,
                        templateArgument.GetTokenDocumentRange(property));
                }

                consumer.ConsumeHighlighting(
                    HighlightingAttributeIds.USAGE_OF_ELEMENT_UNDER_CURSOR,
                    selectedArgument.GetDocumentRange());
            }
        }

        private static void HighlightByNamedPlaceholder(
            HighlightingsConsumer consumer,
            IPsiView psiView,
            ICSharpArgument templateArgument,
            MessageTemplate messageTemplate,
            TreeNodeCollection<ICSharpArgument> arguments)
        {
            if (messageTemplate.NamedProperties != null)
            {
                var(selectedToken, index) = GetSelectedToken(psiView, templateArgument, messageTemplate.NamedProperties);
                if (selectedToken == null)
                {
                    return;
                }

                var argumentIndex = templateArgument.IndexOf() + index + 1;
                if (arguments.Count <= argumentIndex)
                {
                    return;
                }

                consumer.ConsumeHighlighting(
                    HighlightingAttributeIds.USAGE_OF_ELEMENT_UNDER_CURSOR,
                    templateArgument.GetTokenDocumentRange(selectedToken));
                consumer.ConsumeHighlighting(
                    HighlightingAttributeIds.USAGE_OF_ELEMENT_UNDER_CURSOR,
                    arguments[argumentIndex].GetDocumentRange());
            }
            else if (messageTemplate.PositionalProperties != null)
            {
                var(selectedToken, _) = GetSelectedToken(psiView, templateArgument, messageTemplate.PositionalProperties);
                if (selectedToken == null)
                {
                    return;
                }

                if (!selectedToken.TryGetPositionalValue(out int position))
                {
                    return;
                }

                foreach (var property in messageTemplate.PositionalProperties)
                {
                    if (!property.TryGetPositionalValue(out int propertyPosition))
                    {
                        continue;
                    }

                    if (propertyPosition != position)
                    {
                        continue;
                    }

                    consumer.ConsumeHighlighting(
                        HighlightingAttributeIds.USAGE_OF_ELEMENT_UNDER_CURSOR,
                        templateArgument.GetTokenDocumentRange(property));
                }

                var argumentIndex = templateArgument.IndexOf() + position + 1;
                if (arguments.Count <= argumentIndex)
                {
                    return;
                }

                consumer.ConsumeHighlighting(
                    HighlightingAttributeIds.USAGE_OF_ELEMENT_UNDER_CURSOR,
                    arguments[argumentIndex].GetDocumentRange());
            }
        }
    }
}
