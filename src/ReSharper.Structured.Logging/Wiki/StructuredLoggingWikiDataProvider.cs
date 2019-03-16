using System.Collections.Generic;

using JetBrains.Application;
using JetBrains.ReSharper.Feature.Services.Explanatory;

using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.Wiki
{
    [ShellComponent]
    public class StructuredLoggingWikiDataProvider : ICodeInspectionWikiDataProvider
    {
        private const string BaseUrl = "https://github.com/olsh/resharper-structured-logging#";

        private static readonly IDictionary<string, string> AttributeUrlMap = new Dictionary<string, string>
                                                                                  {
                                                                                      {
                                                                                          DuplicateTemplatePropertyWarning
                                                                                              .SeverityId,
                                                                                          $"{BaseUrl}duplicate-template-property"
                                                                                      },
                                                                                      {
                                                                                          ExceptionPassedAsTemplateArgumentWarning
                                                                                              .SeverityId,
                                                                                          $"{BaseUrl}exception-passed-as-a-template-argument"
                                                                                      },
                                                                                      {
                                                                                          TemplateIsNotCompileTimeConstantWarning
                                                                                              .SeverityId,
                                                                                          $"{BaseUrl}message-template-is-not-a-compile-time-constant"
                                                                                      },
                                                                                      {
                                                                                          AnonymousObjectDestructuringWarning
                                                                                              .SeverityId,
                                                                                          $"{BaseUrl}anonymous-objects-must-be-destructured"
                                                                                      },
                                                                                      {
                                                                                          ContextualLoggerWarning
                                                                                              .SeverityId,
                                                                                          $"{BaseUrl}incorrect-type-is-used-for-contextual-logger"
                                                                                      }
                                                                                  };

        public bool TryGetValue(string attributeId, out string url)
        {
            return AttributeUrlMap.TryGetValue(attributeId, out url);
        }
    }
}
