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
        private readonly TemplateParameterNameAttributeProvider _templateParameterNameAttributeProvider;

        public CompileTimeConstantTemplateAnalyzer(CodeAnnotationsCache codeAnnotationsCache)
        {
            _templateParameterNameAttributeProvider = codeAnnotationsCache.GetProvider<TemplateParameterNameAttributeProvider>();
        }

        protected override void Run(
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var templateArgument = element.GetTemplateArgument(_templateParameterNameAttributeProvider);
            if (templateArgument?.Value == null)
            {
                return;
            }

            if (templateArgument.Value.IsConstantValue())
            {
                return;
            }

            consumer.AddHighlighting(new TemplateIsNotCompileTimeConstantWarning(element, templateArgument));
        }
    }
}
