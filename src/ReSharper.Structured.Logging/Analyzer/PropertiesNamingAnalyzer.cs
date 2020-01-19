using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;

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

        public PropertiesNamingAnalyzer(MessageTemplateParser messageTemplateParser)
        {
            _messageTemplateParser = messageTemplateParser;
        }

        protected override void Run(
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var templateArgument = element.GetTemplateArgument();
            if (templateArgument == null)
            {
                return;
            }

            var stringLiteral = StringLiteralAltererUtil.TryCreateStringLiteralByExpression(templateArgument.Value);
            if (stringLiteral == null)
            {
                return;
            }

            var messageTemplate = _messageTemplateParser.Parse(stringLiteral.Expression.GetUnquotedText());
            if (messageTemplate.NamedProperties == null)
            {
                return;
            }

            var namingType = element.GetProject()
                                 ?.GetSolution()
                                 .GetSettingsStore()
                                 .GetValue(StructuredLoggingSettingsAccessor.PropertyNamingType)
                             ?? PropertyNamingType.PascalCase;

            foreach (var property in messageTemplate.NamedProperties)
            {
                if (string.IsNullOrEmpty(property.PropertyName))
                {
                    continue;
                }

                string suggestedName = property.PropertyName;
                switch (namingType)
                {
                    case PropertyNamingType.PascalCase:
                        suggestedName = StringUtil.MakeUpperCamelCaseName(property.PropertyName);
                        break;
                    case PropertyNamingType.CamelCase:
                        suggestedName = StringUtil.MakeUpperCamelCaseName(property.PropertyName).Decapitalize();
                        break;
                    case PropertyNamingType.SnakeCase:
                        suggestedName = StringUtil.MakeUnderscoreCaseName(property.PropertyName);
                        break;
                }

                if (string.Equals(suggestedName, property.PropertyName))
                {
                    continue;
                }

                consumer.AddHighlighting(new InconsistentLogPropertyNamingWarning(stringLiteral, suggestedName, property, stringLiteral.GetTokenDocumentRange(property)));
            }
        }
    }
}
