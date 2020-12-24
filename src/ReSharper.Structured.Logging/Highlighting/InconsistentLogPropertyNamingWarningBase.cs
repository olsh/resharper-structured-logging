namespace ReSharper.Structured.Logging.Highlighting
{
    public class InconsistentLogPropertyNamingWarningBase
    {
        protected const string Message = "Property name does not match naming rules.";

        protected string GetToolTipMessage(string propertyName, string suggestedName)
        {
            return $"Property name '{propertyName}' does not match naming rules'. Suggested name is '{suggestedName}'.";
        }
    }
}
