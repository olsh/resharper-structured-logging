namespace ReSharper.Structured.Logging.Highlighting
{
    public class InconsistentLogPropertyNamingWarningBase
    {
        protected const string Message = "Property name '{0}' does not match naming rules. Suggested name is '{1}'.";

        protected string GetToolTipMessage(string propertyName, string suggestedName)
        {
            return $"Property name '{propertyName}' does not match naming rules. Suggested name is '{suggestedName}'.";
        }
    }
}
