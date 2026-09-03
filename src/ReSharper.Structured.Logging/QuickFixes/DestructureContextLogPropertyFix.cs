using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.QuickFixes;

using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.QuickFixes
{
    /// <summary>
    /// Captures the property by its structure, so that
    /// <c>LogContext.PushProperty("User", new User())</c> becomes
    /// <c>LogContext.PushProperty("User", new User(), destructureObjects: true)</c>.
    /// </summary>
    [QuickFix]
    public class DestructureContextLogPropertyFix : ContextLogPropertyDestructuringFixBase
    {
        public DestructureContextLogPropertyFix([NotNull] ComplexObjectDestructuringInContextWarning error)
            : base(error)
        {
        }

        public override string Text => "Destructure the property";

        protected override bool DestructureObjects => true;
    }
}
