using System;

using JetBrains.Application.Settings;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.Resources;

namespace ReSharper.Structured.Logging.Settings
{
    [OptionsPage(Pid, "Structured Logging", typeof(FeaturesEnvironmentOptionsThemedIcons.StringFormat), ParentId = EnvironmentPage.Pid)]
    public class StructuredLoggingOptionsPage : BeSimpleOptionsPage
    {
        private const string Pid = "StructuredLogging";

        public StructuredLoggingOptionsPage(
            Lifetime lifetime,
            OptionsPageContext optionsPageContext,
            OptionsSettingsSmartContext optionsSettingsSmartContext,
            bool wrapInScrollablePanel = false)
            : base(lifetime, optionsPageContext, optionsSettingsSmartContext, wrapInScrollablePanel)
        {
            AddHeader("Log properties naming style");
            AddComboOptionFromEnum(
                (StructuredLoggingSettings settings) => settings.PropertyNamingType,
                type =>
                    {
                        switch (type)
                        {
                            case PropertyNamingType.PascalCase:
                                return "PascalCase";
                            case PropertyNamingType.CamelCase:
                                return "camelCase";
                            case PropertyNamingType.SnakeCase:
                                return "snake_case";
                            case PropertyNamingType.ElasticNaming:
                                return "elastic.naming";
                            default:
                                throw new ArgumentOutOfRangeException(nameof(type), type, null);
                        }
                    });

            AddHeader("Ignored properties naming");
            AddCommentText("You may specify a regular expression here and if a property matches this expression, then it will be skipped during naming analysis.");
            var ignoredRegex = OptionsSettingsSmartContext
                .GetValueProperty(lifetime, StructuredLoggingSettingsAccessor.IgnoredPropertiesRegex);
            AddControl(ignoredRegex.GetBeTextBox(lifetime));
        }
    }
}
