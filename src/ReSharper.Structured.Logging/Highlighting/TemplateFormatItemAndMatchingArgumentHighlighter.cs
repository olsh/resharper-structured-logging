using System;

using JetBrains.Annotations;
using JetBrains.ReSharper.Daemon.CaretDependentFeatures;
using JetBrains.ReSharper.Feature.Services.Contexts;
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.DataContext;
using JetBrains.ReSharper.Psi.Tree;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Serilog.Events;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Highlighting
{
    [ContainsContextConsumer]
    public class TemplateFormatItemAndMatchingArgumentHighlighter : ContextHighlighterBase
    {
        private const string MatchedElementHighlightingAttribute = DefaultLanguageAttributeIds.MATCHED_FORMAT_STRING_ITEM;

        private readonly MessageTemplateParser _messageTemplate;

        private TemplateFormatItemAndMatchingArgumentHighlighter(MessageTemplateParser messageTemplate)
        {
            _messageTemplate = messageTemplate;
        }

        [AsyncContextConsumer]
        public static Action ProcessContext(
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

            var invocationExpressions = psiView.ContainingNodes<IInvocationExpression>();
            foreach (var invocationExpression in invocationExpressions)
            {
                var templateArgument = invocationExpression.GetTemplateArgument();
                if (templateArgument == null)
                {
                    continue;
                }

                if (templateArgument.IndexOf() > selectedArgument.IndexOf())
                {
                    continue;
                }

                var templateString = templateArgument.TryGetTemplateText();
                if (templateString == null)
                {
                    continue;
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
        }

        private static (PropertyToken token, int index) GetSelectedToken(
            IPsiView psiView,
            ICSharpArgument templateArgument,
            PropertyToken[] properties)
        {
            var selectedTreeRange = psiView.GetSelectedTreeRange(templateArgument);

            var propertyIndex = 0;
            foreach (var property in properties)
            {
                var documentRange = templateArgument.GetTokenInformation(property).DocumentRange;

                if (documentRange.StartOffset.Offset <= selectedTreeRange.StartOffset.Offset
                    && documentRange.EndOffset.Offset >= selectedTreeRange.EndOffset.Offset)
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
                    MatchedElementHighlightingAttribute,
                    templateArgument.GetTokenInformation(property).DocumentRange);
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
                        MatchedElementHighlightingAttribute,
                        templateArgument.GetTokenInformation(property).DocumentRange);
                }
            }

            consumer.ConsumeHighlighting(
                MatchedElementHighlightingAttribute,
                selectedArgument.GetDocumentRange());
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
                    MatchedElementHighlightingAttribute,
                    templateArgument.GetTokenInformation(selectedToken).DocumentRange);
                consumer.ConsumeHighlighting(
                    MatchedElementHighlightingAttribute,
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
                        MatchedElementHighlightingAttribute,
                        templateArgument.GetTokenInformation(property).DocumentRange);
                }

                var argumentIndex = templateArgument.IndexOf() + position + 1;
                if (arguments.Count <= argumentIndex)
                {
                    return;
                }

                consumer.ConsumeHighlighting(
                    MatchedElementHighlightingAttribute,
                    arguments[argumentIndex].GetDocumentRange());
            }
        }
    }
}
