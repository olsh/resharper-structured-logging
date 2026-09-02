using System.Collections.Generic;

using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.BulbActions;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;

using ReSharper.Structured.Logging.Extensions;
using ReSharper.Structured.Logging.Highlighting;

namespace ReSharper.Structured.Logging.QuickFixes
{
    [QuickFix]
    public class ChangeContextualLoggerTypeFix : ModernScopedQuickFixBase
    {
        private readonly ITypeElement _expectedType;

        private readonly ICSharpParameterDeclaration _parameterDeclaration;

        private readonly ITypeUsage _typeArgument;

        public ChangeContextualLoggerTypeFix([NotNull] ContextualLoggerWarning error)
        {
            _typeArgument = error.TypeArgument;
            _expectedType = error.ExpectedType;
            _parameterDeclaration = error.ParameterDeclaration;
        }

        public override string Text => $"Change contextual logger type to '{_expectedType?.ShortName}'";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _typeArgument != null
                   && _typeArgument.IsValid()
                   && _expectedType != null
                   && _expectedType.IsValid();
        }

        /// <inheritdoc />
        protected override ITreeNode TryGetContextTreeNode()
        {
            return _typeArgument;
        }

        protected override IBulbActionCommand ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            // The members have to be located before anything is replaced, because rewriting the
            // parameter type argument invalidates the reference walk used to find them.
            var memberTypeArguments = FindAssignedMemberTypeArguments();

            using (WriteLockCookie.Create())
            {
                ReplaceWithExpectedType(_typeArgument);

                foreach (var memberTypeArgument in memberTypeArguments)
                {
                    ReplaceWithExpectedType(memberTypeArgument);
                }
            }

            return null;
        }

        [CanBeNull]
        private static string GetContextTypeName([CanBeNull] ITypeUsage typeArgument)
        {
            if (typeArgument == null)
            {
                return null;
            }

            return (CSharpTypeFactory.CreateType(typeArgument) as IDeclaredType)?.GetTypeElement()
                ?.GetClrName()
                .FullName;
        }

        private static bool HasSingleDeclarator([NotNull] IFieldDeclaration fieldDeclaration)
        {
            var multipleDeclaration = (fieldDeclaration as IMultipleDeclarationMember)?.MultipleDeclaration;

            return multipleDeclaration == null || multipleDeclaration.Declarators.Count <= 1;
        }

        /// <summary>
        /// Locates the type argument of every field or property the logger parameter is assigned to,
        /// so that the type keeps compiling after the parameter type is changed.
        /// </summary>
        [NotNull]
        private IList<ITypeUsage> FindAssignedMemberTypeArguments()
        {
            var result = new List<ITypeUsage>();
            if (_parameterDeclaration == null || !_parameterDeclaration.IsValid())
            {
                return result;
            }

            var parameter = _parameterDeclaration.DeclaredElement;
            var typeDeclaration = _parameterDeclaration.GetContainingNode<ITypeDeclaration>();
            if (parameter == null || typeDeclaration == null)
            {
                return result;
            }

            // Only members logging the very same wrong type are rewritten. ILogger<T> is covariant,
            // so an ILogger<object> member is legitimate and has to be left alone.
            var wrongTypeName = GetContextTypeName(_typeArgument);
            if (wrongTypeName == null)
            {
                return result;
            }

            foreach (var referenceExpression in typeDeclaration.Descendants<IReferenceExpression>())
            {
                if (referenceExpression.NameIdentifier?.Name != parameter.ShortName)
                {
                    continue;
                }

                if (!parameter.Equals(
                        referenceExpression.Reference.Resolve()
                            .DeclaredElement))
                {
                    continue;
                }

                var typeArgument = GetWrongLoggerTypeArgument(
                    FindMemberTypeUsage(referenceExpression),
                    wrongTypeName);
                if (typeArgument != null && !result.Contains(typeArgument))
                {
                    result.Add(typeArgument);
                }
            }

            return result;
        }

        [CanBeNull]
        private ITypeUsage FindMemberTypeUsage([NotNull] IReferenceExpression referenceExpression)
        {
            // Primary constructor: private readonly ILogger<B> _log = log;
            var initializer = ExpressionInitializerNavigator.GetByValue(referenceExpression);
            if (initializer != null)
            {
                var initializedField = FieldDeclarationNavigator.GetByInitial(initializer);
                if (initializedField != null)
                {
                    return HasSingleDeclarator(initializedField) ? initializedField.TypeUsage : null;
                }

                return PropertyDeclarationNavigator.GetByInitial(initializer)
                    ?.TypeUsage;
            }

            // Regular constructor: _log = log;
            var assignment = AssignmentExpressionNavigator.GetBySource(referenceExpression);
            if (assignment == null || assignment.AssignmentType != AssignmentType.EQ)
            {
                return null;
            }

            var member = (assignment.Dest as IReferenceExpression)?.Reference.Resolve()
                .DeclaredElement;

            // Guards against assigning the logger into a member of some other type.
            if (member == null || !_expectedType.Equals((member as IClrDeclaredElement)?.GetContainingType()))
            {
                return null;
            }

            foreach (var declaration in member.GetDeclarations())
            {
                // Never reach into another file - the quick fix only rewrites what the user can see.
                if (declaration.GetSourceFile() != _parameterDeclaration.GetSourceFile())
                {
                    continue;
                }

                if (declaration is IFieldDeclaration fieldDeclaration)
                {
                    return HasSingleDeclarator(fieldDeclaration) ? fieldDeclaration.TypeUsage : null;
                }

                if (declaration is IPropertyDeclaration propertyDeclaration)
                {
                    return propertyDeclaration.TypeUsage;
                }
            }

            return null;
        }

        [CanBeNull]
        private static ITypeUsage GetWrongLoggerTypeArgument(
            [CanBeNull] ITypeUsage memberTypeUsage,
            [NotNull] string wrongTypeName)
        {
            if (memberTypeUsage == null)
            {
                return null;
            }

            if (!(CSharpTypeFactory.CreateType(memberTypeUsage) is IDeclaredType declaredType)
                || !declaredType.IsGenericMicrosoftExtensionsLogger())
            {
                return null;
            }

            var typeArgument = memberTypeUsage.GetFirstTypeArgumentNode();

            return wrongTypeName.Equals(GetContextTypeName(typeArgument)) ? typeArgument : null;
        }

        private void ReplaceWithExpectedType([NotNull] ITypeUsage node)
        {
            var factory = CSharpElementFactory.GetInstance(node, false);
            var expectedTypeUsage = factory.CreateTypeUsage(TypeFactory.CreateType(_expectedType), node);

            ModificationUtil.ReplaceChild(node, expectedTypeUsage);
        }
    }
}
