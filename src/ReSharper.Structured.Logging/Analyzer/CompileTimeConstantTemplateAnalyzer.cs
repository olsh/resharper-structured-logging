using System;

using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Analyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression))]
    public class CompileTimeConstantTemplateAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private readonly Lazy<TemplateParameterNameAttributeProvider> _templateParameterNameAttributeProvider;

        public CompileTimeConstantTemplateAnalyzer(CodeAnnotationsCache codeAnnotationsCache)
        {
            _templateParameterNameAttributeProvider = codeAnnotationsCache.GetLazyProvider<TemplateParameterNameAttributeProvider>();
        }

        protected override void Run(
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var templateArgument = element.GetTemplateArgument(_templateParameterNameAttributeProvider.Value);
            if (templateArgument?.Value == null)
            {
                return;
            }

            if (templateArgument.Value.IsConstantValue())
            {
                return;
            }

            // A ZLogger 2.x template is an interpolated string by design: the handler consumes the holes
            // one by one, nothing formats the string before the logger sees it
            if (templateArgument.Value.IsZLoggerTemplateHandler())
            {
                return;
            }

            consumer.AddHighlighting(new TemplateIsNotCompileTimeConstantWarning(element, templateArgument));
        }
    }
}
