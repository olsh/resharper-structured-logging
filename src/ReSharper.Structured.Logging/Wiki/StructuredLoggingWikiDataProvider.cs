using System.Collections.Generic;

using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.Feature.Services.Explanatory;

using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Wiki
{
    [ShellComponent(Instantiation.DemandAnyThreadSafe)]
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
                                                                                      },
                                                                                      {
                                                                                          InconsistentLogPropertyNamingWarning.SeverityId,
                                                                                          CreateSeverityUrl(InconsistentLogPropertyNamingWarning.SeverityId)
                                                                                      },
                                                                                      {
                                                                                          LogMessageIsSentenceWarning.SeverityId,
                                                                                          CreateSeverityUrl(LogMessageIsSentenceWarning.SeverityId)
                                                                                      },
                                                                                      {
                                                                                          ComplexObjectDestructuringInContextWarning.SeverityId,
                                                                                          CreateSeverityUrl(ComplexObjectDestructuringInContextWarning.SeverityId)
                                                                                      },
                                                                                      {
                                                                                          InconsistentContextLogPropertyNamingWarning.SeverityId,
                                                                                          CreateSeverityUrl(InconsistentContextLogPropertyNamingWarning.SeverityId)
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
