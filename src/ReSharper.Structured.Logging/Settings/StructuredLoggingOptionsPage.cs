using System;

using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.Application.UI.Options.OptionsDialog;
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
                                return "PacalCase";
                            case PropertyNamingType.CamelCase:
                                return "camelCase";
                            case PropertyNamingType.SnakeCase:
                                return "snake_case";
                            default:
                                throw new ArgumentOutOfRangeException(nameof(type), type, null);
                        }
                    });
        }
    }
}
