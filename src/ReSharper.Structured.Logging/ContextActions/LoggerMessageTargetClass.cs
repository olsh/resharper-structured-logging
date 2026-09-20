using System.Collections.Generic;

using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

namespace ReSharper.Structured.Logging.ContextActions
{
    /// <summary>
    /// Finds the <c>static partial class</c> the generated method belongs in, or creates one.
    /// </summary>
    /// <remarks>
    /// The class is created beside the type doing the logging rather than inside it, because the generator
    /// needs every enclosing type to be partial, and nesting would force a <c>partial</c> modifier onto a
    /// class the action has no other reason to touch.
    /// </remarks>
    public static class LoggerMessageTargetClass
    {
        public const string DefaultClassName = "Log";

        private static readonly IClrTypeName LoggerMessageAttributeFqn =
            new ClrTypeName("Microsoft.Extensions.Logging.LoggerMessageAttribute");

        /// <summary>
        /// Creates <c>internal static partial class Log</c> holding <paramref name="memberDeclaration"/> and
        /// puts it beside the outermost type containing the call, or returns <c>null</c> when there is nowhere
        /// to put it.
        /// </summary>
        /// <remarks>
        /// The member joins the class while the class is still detached, so that the whole declaration reaches
        /// the file in one piece. Adding it afterwards leaves it indented as the factory wrote it, and no
        /// amount of formatting the inserted node puts that right.
        /// </remarks>
        [CanBeNull]
        public static IClassLikeDeclaration Create(
            [NotNull] IInvocationExpression invocationExpression,
            [NotNull] CSharpElementFactory factory,
            [NotNull] string name,
            [NotNull] IClassMemberDeclaration memberDeclaration)
        {
            var anchor = FindOutermostTypeDeclaration(invocationExpression);
            var holder = anchor?.GetContainingNode<ICSharpTypeAndNamespaceHolderDeclaration>();
            if (anchor == null || holder == null)
            {
                return null;
            }

            if (!(factory.CreateTypeMemberDeclaration($"internal static partial class {name} {{ }}") is
                    IClassDeclaration declaration))
            {
                return null;
            }

            declaration.AddClassMemberDeclaration(memberDeclaration);

            return holder.AddTypeDeclarationAfter(declaration, anchor) as IClassLikeDeclaration;
        }

        /// <summary>
        /// A class name no type of the namespace has taken. The suitable classes have already been ruled out
        /// by <see cref="TryFind"/>, so a taken <c>Log</c> here belongs to something else entirely.
        /// </summary>
        [NotNull]
        public static string FindFreeClassName([NotNull] IInvocationExpression invocationExpression)
        {
            var takenNames = new JetHashSet<string>();
            foreach (var typeElement in GetNamespaceTypeElements(invocationExpression))
            {
                takenNames.Add(typeElement.ShortName);
            }

            if (!takenNames.Contains(DefaultClassName))
            {
                return DefaultClassName;
            }

            var suffix = 2;
            while (takenNames.Contains(DefaultClassName + suffix))
            {
                suffix++;
            }

            return DefaultClassName + suffix;
        }

        /// <summary>
        /// An existing class the generated method can join: one declared <c>static partial</c> whose members
        /// already carry <c>[LoggerMessage]</c>. The file being edited is preferred, so that a call converted
        /// next to such a class keeps it company, and only then the rest of the namespace.
        /// </summary>
        [CanBeNull]
        public static IClassLikeDeclaration TryFind([NotNull] IInvocationExpression invocationExpression)
        {
            var psiModule = invocationExpression.GetPsiModule();
            var sourceFile = invocationExpression.GetSourceFile();
            IClassLikeDeclaration candidate = null;

            // Only the types of the namespace around the call are considered, and a namespace holds no nested
            // types, so the unqualified name the action writes at the call site always resolves. A class in
            // another namespace, or one nested in some other type, would need qualifying or would not resolve.
            foreach (var typeElement in GetNamespaceTypeElements(invocationExpression))
            {
                // A generic class would have to be named with its type arguments at the call site, and the
                // action writes the bare name
                if (typeElement.TypeParametersCount > 0)
                {
                    continue;
                }

                foreach (var declaration in typeElement.GetDeclarations())
                {
                    // Only a file of this project, so the action never reaches into a dependency
                    if (!(declaration is IClassDeclaration classDeclaration)
                        || !Equals(declaration.GetPsiModule(), psiModule)
                        || !IsSuitable(classDeclaration))
                    {
                        continue;
                    }

                    // A class declared in the file being edited keeps the generated method next to the call
                    if (Equals(declaration.GetSourceFile(), sourceFile))
                    {
                        return classDeclaration;
                    }

                    candidate = candidate ?? classDeclaration;
                }
            }

            return candidate;
        }

        /// <summary>
        /// The type the new class sits beside. A call inside a nested type still puts it at namespace level,
        /// where it needs no enclosing type to be made partial.
        /// </summary>
        [CanBeNull]
        private static ICSharpTypeDeclaration FindOutermostTypeDeclaration(
            [NotNull] IInvocationExpression invocationExpression)
        {
            var typeDeclaration = invocationExpression.GetContainingNode<ICSharpTypeDeclaration>();
            if (typeDeclaration == null)
            {
                return null;
            }

            var enclosingTypeDeclaration = typeDeclaration.GetContainingNode<ICSharpTypeDeclaration>();
            while (enclosingTypeDeclaration != null)
            {
                typeDeclaration = enclosingTypeDeclaration;
                enclosingTypeDeclaration = typeDeclaration.GetContainingNode<ICSharpTypeDeclaration>();
            }

            return typeDeclaration;
        }

        [NotNull]
        private static IEnumerable<ITypeElement> GetNamespaceTypeElements(
            [NotNull] IInvocationExpression invocationExpression)
        {
            var containingNamespace = FindOutermostTypeDeclaration(invocationExpression)
                ?.DeclaredElement?.GetContainingNamespace();
            if (containingNamespace == null)
            {
                return EmptyList<ITypeElement>.Instance;
            }

            var psiModule = invocationExpression.GetPsiModule();
            var symbolScope = invocationExpression.GetPsiServices()
                .Symbols.GetSymbolScope(psiModule, false, true);

            return containingNamespace.GetNestedTypeElements(symbolScope);
        }

        private static bool HasLoggerMessageMember([NotNull] IClassDeclaration classDeclaration)
        {
            foreach (var memberDeclaration in classDeclaration.MemberDeclarations)
            {
                if (!(memberDeclaration is IAttributesOwnerDeclaration attributesOwnerDeclaration))
                {
                    continue;
                }

                foreach (var attribute in attributesOwnerDeclaration.Attributes)
                {
                    var attributeType = attribute.TypeReference?.Resolve()
                        .DeclaredElement as ITypeElement;
                    if (attributeType != null && LoggerMessageAttributeFqn.Equals(attributeType.GetClrName()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsSuitable([NotNull] IClassDeclaration classDeclaration)
        {
            // The generator needs the class partial, and a class that already generates log methods is the one
            // this project has settled on
            return classDeclaration.IsStatic && classDeclaration.IsPartial
                                             && HasLoggerMessageMember(classDeclaration);
        }
    }
}
