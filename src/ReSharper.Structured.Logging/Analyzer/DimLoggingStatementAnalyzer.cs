using System;

using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Settings;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression), HighlightingTypes = new[] { typeof(DimmedLoggingStatementHighlighting) })]
    public class DimLoggingStatementAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private readonly Lazy<TemplateParameterNameAttributeProvider> _templateParameterNameAttributeProvider;

        public DimLoggingStatementAnalyzer(CodeAnnotationsCache codeAnnotationsCache)
        {
            _templateParameterNameAttributeProvider = codeAnnotationsCache.GetLazyProvider<TemplateParameterNameAttributeProvider>();
        }

        protected override void Run(
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            if (!data.SettingsStore.GetValue(StructuredLoggingSettingsAccessor.DimLoggingStatements))
            {
                return;
            }

            if (element.GetTemplateParameterName(_templateParameterNameAttributeProvider.Value) == null)
            {
                return;
            }

            var statement = GetDimmableStatement(element);
            if (statement == null)
            {
                return;
            }

            consumer.AddHighlighting(new DimmedLoggingStatementHighlighting(statement.GetDocumentRange()));
        }

        /// <summary>
        /// Only a statement that is nothing but the logging call is dimmed, so that a logging call feeding a
        /// larger expression, such as <c>var written = Log.Write(...) &amp;&amp; Save()</c>, keeps its colors.
        /// </summary>
        private static IExpressionStatement GetDimmableStatement(IInvocationExpression invocationExpression)
        {
            var statement = ExpressionStatementNavigator.GetByExpression(invocationExpression);
            if (statement != null)
            {
                return statement;
            }

            var awaitExpression = AwaitExpressionNavigator.GetByTask(invocationExpression);

            return awaitExpression == null ? null : ExpressionStatementNavigator.GetByExpression(awaitExpression);
        }
    }
}
