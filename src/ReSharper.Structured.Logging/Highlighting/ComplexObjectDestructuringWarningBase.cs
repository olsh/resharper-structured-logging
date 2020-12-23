namespace ReSharper.Structured.Logging.Highlighting
{
    public abstract class ComplexObjectDestructuringWarningBase
    {
        protected const string Message = "Complex objects with default ToString() implementation probably need to be destructured";
    }
}
