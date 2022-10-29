using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Serilog.Parsing;
using ReSharper.Structured.Logging.Settings;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression))]
    public class PropertiesNamingAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private readonly MessageTemplateParser _messageTemplateParser;

        private readonly TemplateParameterNameAttributeProvider _templateParameterNameAttributeProvider;

        public PropertiesNamingAnalyzer(MessageTemplateParser messageTemplateParser, CodeAnnotationsCache codeAnnotationsCache)
        {
            _messageTemplateParser = messageTemplateParser;
            _templateParameterNameAttributeProvider = codeAnnotationsCache.GetProvider<TemplateParameterNameAttributeProvider>();
        }

        protected override void Run(
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var namingType = element.GetProject()
                                 ?.GetSolution()
                                 .GetSettingsStore()
                                 .GetValue(StructuredLoggingSettingsAccessor.PropertyNamingType)
                             ?? PropertyNamingType.PascalCase;

            CheckPropertiesInTemplate(element, consumer, namingType);
            CheckPropertiesInContext(element, consumer, namingType);
        }

        private void CheckPropertiesInTemplate(
            IInvocationExpression element,
            IHighlightingConsumer consumer,
            PropertyNamingType namingType)
        {
            var templateArgument = element.GetTemplateArgument(_templateParameterNameAttributeProvider);
            var templateText = templateArgument?.TryGetTemplateText();
            if (templateText == null)
            {
                return;
            }

            var messageTemplate = _messageTemplateParser.Parse(templateText);
            if (messageTemplate.NamedProperties == null)
            {
                return;
            }

            foreach (var property in messageTemplate.NamedProperties)
            {
                if (string.IsNullOrEmpty(property.PropertyName))
                {
                    continue;
                }

                var suggestedName = GetSuggestedName(property.PropertyName, namingType);
                if (string.Equals(suggestedName, property.PropertyName))
                {
                    continue;
                }

                consumer.AddHighlighting(
                    new InconsistentLogPropertyNamingWarning(templateArgument.GetTokenInformation(property), property,
                        suggestedName));
            }
        }

        private void CheckPropertiesInContext(
            IInvocationExpression element,
            IHighlightingConsumer consumer,
            PropertyNamingType namingType)
        {
            if (!element.IsSerilogContextPushPropertyMethod())
            {
                return;
            }

            if (element.ArgumentList.Arguments.Count < 1)
            {
                return;
            }

            var propertyArgument = element.ArgumentList.Arguments[0];
            var propertyName = propertyArgument.Value.ConstantValue.Value as string;
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            var suggestedName = GetSuggestedName(propertyName, namingType);
            if (string.Equals(propertyName, suggestedName))
            {
                return;
            }

            consumer.AddHighlighting(new InconsistentContextLogPropertyNamingWarning(propertyArgument, propertyName, suggestedName));
        }

        private string GetSuggestedName(string propertyName, PropertyNamingType namingType)
        {
            switch (namingType)
            {
                case PropertyNamingType.PascalCase:
                    return StringUtil.MakeUpperCamelCaseName(propertyName);
                case PropertyNamingType.CamelCase:
                    return StringUtil.MakeUpperCamelCaseName(propertyName).Decapitalize();
                case PropertyNamingType.SnakeCase:
                    return StringUtil.MakeUnderscoreCaseName(propertyName);
                case PropertyNamingType.ElasticNaming:
                    return StringUtil.MakeUnderscoreCaseName(propertyName).Replace('_', '.');
                default:
                    return propertyName;
            }
        }
    }
}
