using System.Collections.Generic;

using JetBrains.Application;
using JetBrains.ReSharper.Feature.Services.Explanatory;

using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Wiki
{
    [ShellComponent]
    public class StructuredLoggingWikiDataProvider : ICodeInspectionWikiDataProvider
    {
        private static readonly IDictionary<string, string> AttributeUrlMap = new Dictionary<string, string>
                                                                                  {
                                                                                      {
                                                                                          DuplicateTemplatePropertyWarning.SeverityId,
                                                                                          CreateSeverityUrl(DuplicateTemplatePropertyWarning.SeverityId)
                                                                                      },
                                                                                      {
                                                                                          ExceptionPassedAsTemplateArgumentWarning.SeverityId,
                                                                                          CreateSeverityUrl(ExceptionPassedAsTemplateArgumentWarning.SeverityId)
                                                                                      },
                                                                                      {
                                                                                          TemplateIsNotCompileTimeConstantWarning.SeverityId,
                                                                                          CreateSeverityUrl(TemplateIsNotCompileTimeConstantWarning.SeverityId)
                                                                                      },
                                                                                      {
                                                                                          AnonymousObjectDestructuringWarning.SeverityId,
                                                                                          CreateSeverityUrl(AnonymousObjectDestructuringWarning.SeverityId)
                                                                                      },
                                                                                      {
                                                                                          ContextualLoggerWarning.SeverityId,
                                                                                          CreateSeverityUrl(ContextualLoggerWarning.SeverityId)
                                                                                      },
                                                                                      {
                                                                                          ComplexObjectDestructuringWarning.SeverityId,
                                                                                          CreateSeverityUrl(ComplexObjectDestructuringWarning.SeverityId)
                                                                                      },
                                                                                      {
                                                                                          PositionalPropertyUsedWarning.SeverityId,
                                                                                          CreateSeverityUrl(PositionalPropertyUsedWarning.SeverityId)
                                                                                      }
                                                                                  };

        public bool TryGetValue(string attributeId, out string url)
        {
            return AttributeUrlMap.TryGetValue(attributeId, out url);
        }

        private static string CreateSeverityUrl(string severityId)
        {
            return $"https://github.com/olsh/resharper-structured-logging/blob/master/rules/{severityId}.md";
        }
    }
}
