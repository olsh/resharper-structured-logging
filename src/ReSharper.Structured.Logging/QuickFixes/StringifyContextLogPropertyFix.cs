using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.QuickFixes;

using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.QuickFixes
{
    /// <summary>
    /// Keeps the property a scalar rendered by <c>ToString()</c>, so that
    /// <c>LogContext.PushProperty("User", new User())</c> becomes
    /// <c>LogContext.PushProperty("User", new User(), destructureObjects: false)</c>. Offered next to
    /// <see cref="DestructureContextLogPropertyFix"/>, since both are compliant and only the author
    /// knows which one was meant.
    /// </summary>
    [QuickFix]
    public class StringifyContextLogPropertyFix : ContextLogPropertyDestructuringFixBase
    {
        public StringifyContextLogPropertyFix([NotNull] ComplexObjectDestructuringInContextWarning error)
            : base(error)
        {
        }

        public override string Text => "Log the property as a string";

        protected override bool DestructureObjects => false;
    }
}
