using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.Structured.Logging.ContextActions
{
    /// <summary>
    /// One parameter of the generated <c>[LoggerMessage]</c> method, together with the expression the
    /// original call passed for it, which becomes the argument of the generated call.
    /// </summary>
    public sealed class LoggerMessageParameter
    {
        public LoggerMessageParameter([NotNull] string name, [NotNull] IType type, [NotNull] ICSharpExpression value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        [NotNull]
        public string Name { get; }

        [NotNull]
        public IType Type { get; }

        [NotNull]
        public ICSharpExpression Value { get; }
    }
}
