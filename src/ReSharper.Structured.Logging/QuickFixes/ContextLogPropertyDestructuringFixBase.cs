using System;

using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.QuickFixes
{
    /// <summary>
    /// States how a context property is captured by appending the destructuring flag that
    /// <c>LogContext.PushProperty</c> leaves optional.
    /// </summary>
    public abstract class ContextLogPropertyDestructuringFixBase : QuickFixBase
    {
        // The warning is only reported for Serilog's LogContext.PushProperty, so the flag is
        // always declared under this name
        private const string DestructureObjectsParameterName = "destructureObjects";

        private readonly IInvocationExpression _invocationExpression;

        protected ContextLogPropertyDestructuringFixBase([NotNull] ComplexObjectDestructuringInContextWarning error)
        {
            _invocationExpression = error.InvocationExpression;
        }

        /// <summary>
        /// The value the appended flag is given.
        /// </summary>
        protected abstract bool DestructureObjects { get; }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            // The flag is appended, so the call has to be the two argument form the analyzer reports
            return _invocationExpression.IsValid() && _invocationExpression.ArgumentList.Arguments.Count == 2;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_invocationExpression, false);
                var arguments = _invocationExpression.ArgumentList.Arguments;
                var value = factory.CreateExpression(DestructureObjects ? "true" : "false");

                // A bare literal says nothing at the call site, the name is what makes the choice readable
                _invocationExpression.AddArgumentAfter(
                    factory.CreateArgument(ParameterKind.VALUE, DestructureObjectsParameterName, value),
                    arguments[arguments.Count - 1]);
            }

            return null;
        }
    }
}
