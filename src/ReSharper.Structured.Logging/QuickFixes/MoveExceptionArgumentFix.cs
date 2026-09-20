using System;

using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

using ReSharper.Structured.Logging.Highlighting;
using ReSharper.Structured.Logging.Models;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.QuickFixes
{
    [QuickFix]
    public class MoveExceptionArgumentFix : QuickFixBase
    {
        private readonly ICSharpArgument _exceptionArgument;

        private readonly bool _exceptionArgumentOccupied;

        [CanBeNull] private readonly ICSharpExpression _exceptionExpression;

        private readonly IInvocationExpression _invocationExpression;

        [CanBeNull] private readonly PropertyToken _namedProperty;

        private readonly ICSharpArgument _templateArgument;

        [CanBeNull] private readonly MessageTemplateTokenInformation _tokenInformation;

        public MoveExceptionArgumentFix([NotNull] ExceptionPassedAsTemplateArgumentWarning error)
        {
            _exceptionArgument = error.ExceptionArgument;
            _exceptionExpression = error.ExceptionArgument.Value;
            _templateArgument = error.TemplateArgument;
            _invocationExpression = error.InvocationExpression;
            _tokenInformation = error.TokenInformation;
            _namedProperty = error.NamedProperty;
            _exceptionArgumentOccupied = error.ExceptionArgumentOccupied;
        }

        /// <summary>
        /// The argument logs only a piece of the exception, so the exception to pass on is the receiver the
        /// text was read from rather than the argument itself.
        /// </summary>
        public MoveExceptionArgumentFix([NotNull] ExceptionLoggedAsTextWarning error)
        {
            _exceptionArgument = error.ExceptionTextArgument;
            _exceptionExpression = error.ExceptionExpression;
            _templateArgument = error.TemplateArgument;
            _invocationExpression = error.InvocationExpression;
            _tokenInformation = error.TokenInformation;
            _namedProperty = error.NamedProperty;
            _exceptionArgumentOccupied = false;
        }

        public override string Text => "Pass exception to the exception argument";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            // Moving the exception when another one already fills the dedicated argument
            // would pass two exceptions, which does not compile
            return !_exceptionArgumentOccupied
                   && !HasRemainingNamedArgument()
                   && _invocationExpression.IsValid()
                   && _exceptionArgument.IsValid()
                   && _templateArgument.IsValid();
        }

        /// <summary>
        /// Reports whether an argument the call keeps is a named one. Inserting the exception shifts every
        /// argument after it one place along, and dropping a hole value resolves the call to a different
        /// overload, whose hole parameters carry different names: Serilog's <c>propertyValue0</c> and
        /// <c>propertyValue1</c> become a single <c>propertyValue</c>. Either is enough to stop a name from
        /// binding, so the fix stays away from such a call rather than rewriting it into code that does not
        /// compile. The argument being moved does not count, as its name goes away with it.
        /// </summary>
        private bool HasRemainingNamedArgument()
        {
            foreach (var argument in _invocationExpression.ArgumentList.Arguments)
            {
                if (argument.NameIdentifier != null && !Equals(argument, _exceptionArgument))
                {
                    return true;
                }
            }

            return false;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_invocationExpression, false);

                // The hole the exception filled is only known when the template is a literal the fix can rewrite,
                // otherwise the argument is moved and the message is left alone
                if (_tokenInformation?.StringLiteral != null && _namedProperty != null)
                {
                    var literalExpression = _tokenInformation.StringLiteral.Expression;
                    var templateText = RemoveProperty(
                        literalExpression.GetUnquotedText(),
                        _tokenInformation.RelativeStartIndex,
                        _namedProperty.Length);

                    ModificationUtil.ReplaceChild(literalExpression, factory.CreateExpression($"\"{templateText}\""));
                }

                if (_exceptionExpression != null)
                {
                    _invocationExpression.AddArgumentBefore(
                        factory.CreateArgument(ParameterKind.VALUE, _exceptionExpression),
                        _templateArgument);
                }

                _invocationExpression.RemoveArgument(_exceptionArgument);
            }

            return null;
        }

        /// <summary>
        /// Removes the hole from the template and collapses the separator it leaves behind, so that
        /// "exceeded {Exception}" becomes "exceeded" rather than "exceeded ".
        /// </summary>
        private static string RemoveProperty([NotNull] string templateText, int startIndex, int length)
        {
            var text = templateText.Remove(startIndex, length);
            if (startIndex == 0 || templateText[startIndex - 1] != ' ')
            {
                return text;
            }

            // A hole between two words leaves a double space, a trailing hole leaves a trailing space
            if (startIndex == text.Length || text[startIndex] == ' ')
            {
                return text.Remove(startIndex - 1, 1);
            }

            return text;
        }
    }
}
