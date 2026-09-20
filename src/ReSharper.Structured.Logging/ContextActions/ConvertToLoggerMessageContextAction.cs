using System.Collections.Generic;
using System.Globalization;
using System.Text;

using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.BulbActions;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Hotspots;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Templates;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;

using ReSharper.Structured.Logging.Caching;
using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.ContextActions
{
    /// <summary>
    /// Turns a Microsoft.Extensions.Logging call into a source-generated <c>[LoggerMessage]</c> method and a
    /// call to it, which is the rewrite CA1848 asks for and offers no fix for.
    /// </summary>
    /// <remarks>
    /// This is a context action rather than an inspection on purpose. Every rule this plugin ships reports a
    /// defect, whereas moving to the generator is a performance preference that holds for every logging call
    /// there is, so highlighting it would mark every logging statement in a solution.
    /// </remarks>
    [ContextAction(
        GroupType = typeof(CSharpContextActions),
        Name = ActionName,
        Description =
            "Converts a Microsoft.Extensions.Logging call into a source-generated [LoggerMessage] method.",
        Priority = 1)]
    public class ConvertToLoggerMessageContextAction : ModernContextActionBase
    {
        private const string ActionName = "Convert to LoggerMessage";

        private const string ExceptionFqn = "System.Exception";

        private const string LogLevelFqn = "Microsoft.Extensions.Logging.LogLevel";

        private const string LoggerFqn = "Microsoft.Extensions.Logging.ILogger";

        private const string LoggerMessageAttributeFqn = "Microsoft.Extensions.Logging.LoggerMessageAttribute";

        [NotNull] private readonly ICSharpContextActionDataProvider _provider;

        [CanBeNull] private LoggerMessageCallModel _model;

        public ConvertToLoggerMessageContextAction([NotNull] ICSharpContextActionDataProvider provider)
        {
            _provider = provider;
        }

        public override string Text => ActionName;

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _model = TryBuildModel();

            return _model != null;
        }

        protected override IBulbActionCommand ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var model = _model;
            if (model == null || !model.IsValid())
            {
                return null;
            }

            var invocationExpression = model.InvocationExpression;
            var factory = CSharpElementFactory.GetInstance(invocationExpression, false);
            var psiModule = invocationExpression.GetPsiModule();

            using (WriteLockCookie.Create())
            {
                // A class that already exists keeps the method; only then is a new one worth creating
                var existingClass = LoggerMessageTargetClass.TryFind(invocationExpression);
                var className = existingClass?.DeclaredName
                                ?? LoggerMessageTargetClass.FindFreeClassName(invocationExpression);

                var methodName = LoggerMessageMethodNameSuggestion.MakeUnique(model.SuggestedMethodName, existingClass);
                var methodDeclaration = CreateMethodDeclaration(factory, psiModule, model, methodName);
                if (methodDeclaration == null)
                {
                    return null;
                }

                // The call is built before the original is replaced, since its expressions come from it
                var callExpression = CreateCallExpression(factory, model, className, methodName);

                // A new class reaches the file already holding the method, so that the whole declaration is laid
                // out in one piece; an existing class simply takes the method
                IClassLikeDeclaration targetClass;
                if (existingClass == null)
                {
                    targetClass = LoggerMessageTargetClass.Create(
                        invocationExpression,
                        factory,
                        className,
                        methodDeclaration);
                    if (targetClass == null)
                    {
                        return null;
                    }
                }
                else
                {
                    targetClass = existingClass;
                    targetClass.AddClassMemberDeclaration(methodDeclaration);
                }

                var addedCall = ModificationUtil.ReplaceChild(invocationExpression, callExpression);

                // The factory writes the declaration in its own shape, so the file decides how it should look.
                // The whitespace that indents a member belongs to its parent rather than to the member, so a
                // class this action created is formatted over its text range; formatting the node alone would
                // tidy the signature and leave the body indented as the factory wrote it.
                if (existingClass == null)
                {
                    CodeFormatterHelper.FormatFileRange(
                        targetClass.GetContainingFile(),
                        targetClass.GetTreeTextRange(),
                        CodeFormatProfile.DEFAULT,
                        progress,
                        OuterSpaceFormatType.AlwaysFormat,
                        OuterSpaceFormatType.AlwaysFormat);
                }

                var addedMethod = FindMethodDeclaration(targetClass, methodName);
                if (addedMethod != null)
                {
                    CodeFormatterHelper.FormatNode(addedMethod, progress);
                }

                return CreateRenameHotspotCommand(addedMethod, addedCall, methodName);
            }
        }

        /// <summary>
        /// The generated method inside the target class. Inserting a class copies its subtree, so the
        /// declaration that ends up in the file is not the one that was handed to the factory.
        /// </summary>
        [CanBeNull]
        private static IMethodDeclaration FindMethodDeclaration(
            [NotNull] IClassLikeDeclaration targetClass,
            [NotNull] string methodName)
        {
            foreach (var memberDeclaration in targetClass.MemberDeclarations)
            {
                if (memberDeclaration is IMethodDeclaration methodDeclaration
                    && methodDeclaration.DeclaredName == methodName)
                {
                    return methodDeclaration;
                }
            }

            return null;
        }

        /// <summary>
        /// Appends one parameter to the signature under construction, as a <c>$n</c> placeholder so that the
        /// factory imports the type rather than writing its full name out.
        /// </summary>
        private static void AppendParameter(
            [NotNull] StringBuilder signature,
            [NotNull] ICollection<object> arguments,
            [CanBeNull] IType type,
            [NotNull] string name)
        {
            if (type == null)
            {
                return;
            }

            if (arguments.Count > 0)
            {
                signature.Append(", ");
            }

            signature.Append('$')
                .Append(arguments.Count)
                .Append(' ')
                .Append(name);
            arguments.Add(type);
        }

        /// <summary>
        /// The <c>[LoggerMessage]</c> attribute for the call. The event id is only written when the call
        /// carried one, and the level only when it is constant; a computed level travels as a parameter
        /// instead, which the generator binds by type.
        /// </summary>
        [CanBeNull]
        private static IAttribute CreateAttribute(
            [NotNull] CSharpElementFactory factory,
            [NotNull] IPsiModule psiModule,
            [NotNull] LoggerMessageCallModel model)
        {
            var attributeType = TypeFactory.CreateTypeByCLRName(LoggerMessageAttributeFqn, psiModule)
                .GetTypeElement();
            if (attributeType == null)
            {
                return null;
            }

            var namedArguments = new List<Pair<string, ICSharpExpression>>(3);
            if (model.EventId.HasValue)
            {
                namedArguments.Add(
                    new Pair<string, ICSharpExpression>(
                        "EventId",
                        factory.CreateExpression(model.EventId.Value.ToString(CultureInfo.InvariantCulture))));
            }

            if (model.LevelName != null)
            {
                namedArguments.Add(
                    new Pair<string, ICSharpExpression>(
                        "Level",
                        factory.CreateExpression(
                            "$0." + model.LevelName,
                            TypeFactory.CreateTypeByCLRName(LogLevelFqn, psiModule))));
            }

            namedArguments.Add(
                new Pair<string, ICSharpExpression>(
                    "Message",
                    factory.CreateExpression(model.MessageLiteralText)));

            return factory.CreateAttribute(
                attributeType,
                EmptyArray<ICSharpExpression>.Instance,
                namedArguments.ToArray());
        }

        /// <summary>
        /// The call that replaces the original one. The arguments follow the order of the generated signature.
        /// </summary>
        [NotNull]
        private static ICSharpExpression CreateCallExpression(
            [NotNull] CSharpElementFactory factory,
            [NotNull] LoggerMessageCallModel model,
            [NotNull] string className,
            [NotNull] string methodName)
        {
            var arguments = new List<object>();
            var call = new StringBuilder(className).Append('.')
                .Append(methodName)
                .Append('(');

            foreach (var expression in CollectCallArguments(model))
            {
                if (arguments.Count > 0)
                {
                    call.Append(", ");
                }

                call.Append('$')
                    .Append(arguments.Count);
                arguments.Add(expression);
            }

            call.Append(')');

            return factory.CreateExpression(call.ToString(), arguments.ToArray());
        }

        [NotNull]
        private static IEnumerable<ICSharpExpression> CollectCallArguments([NotNull] LoggerMessageCallModel model)
        {
            yield return model.LoggerExpression;

            if (model.LevelExpression != null)
            {
                yield return model.LevelExpression;
            }

            if (model.ExceptionExpression != null)
            {
                yield return model.ExceptionExpression;
            }

            foreach (var parameter in model.Parameters)
            {
                yield return parameter.Value;
            }
        }

        [CanBeNull]
        private static IClassMemberDeclaration CreateMethodDeclaration(
            [NotNull] CSharpElementFactory factory,
            [NotNull] IPsiModule psiModule,
            [NotNull] LoggerMessageCallModel model,
            [NotNull] string methodName)
        {
            var arguments = new List<object>();
            var signature = new StringBuilder("public static partial void ").Append(methodName)
                .Append('(');

            AppendParameter(signature, arguments, TypeFactory.CreateTypeByCLRName(LoggerFqn, psiModule), "logger");
            if (model.LevelExpression != null)
            {
                AppendParameter(
                    signature,
                    arguments,
                    TypeFactory.CreateTypeByCLRName(LogLevelFqn, psiModule),
                    "level");
            }

            if (model.ExceptionExpression != null)
            {
                AppendParameter(
                    signature,
                    arguments,
                    TypeFactory.CreateTypeByCLRName(ExceptionFqn, psiModule),
                    "exception");
            }

            foreach (var parameter in model.Parameters)
            {
                AppendParameter(signature, arguments, parameter.Type, parameter.Name);
            }

            signature.Append(");");

            if (!(factory.CreateTypeMemberDeclaration(signature.ToString(), arguments.ToArray()) is IMethodDeclaration
                    methodDeclaration))
            {
                return null;
            }

            var attribute = CreateAttribute(factory, psiModule, model);
            if (attribute == null)
            {
                return null;
            }

            methodDeclaration.AddAttributeAfter(attribute, null);

            return methodDeclaration;
        }

        /// <summary>
        /// Leaves the caret on the new method name as an editable hotspot spanning both the declaration and
        /// the call, so that the name the template suggested can be replaced in one go.
        /// </summary>
        [CanBeNull]
        private static IBulbActionCommand CreateRenameHotspotCommand(
            [CanBeNull] IClassMemberDeclaration methodDeclaration,
            [CanBeNull] ITreeNode callExpression,
            [NotNull] string methodName)
        {
            var ranges = new List<DocumentRange>(2);
            AddCallNameRange(ranges, callExpression, methodName);
            AddNameRange(ranges, (methodDeclaration as IMethodDeclaration)?.NameIdentifier);

            if (ranges.Count == 0)
            {
                return null;
            }

            var field = new TemplateField(methodName, new NameSuggestionsExpression(new[] { methodName }), 0);

            return BulbActionCommands.ShowHotspotSessionAndLeaveCaretWhereItWas(
                new[] { new HotspotInfo(field, ranges) });
        }

        /// <summary>
        /// Adds the range of the method name inside the generated call. The reference is looked up by name
        /// rather than taken off the invocation directly, so that it is found whatever shape the factory
        /// handed back.
        /// </summary>
        private static void AddCallNameRange(
            [NotNull] ICollection<DocumentRange> ranges,
            [CanBeNull] ITreeNode callExpression,
            [NotNull] string methodName)
        {
            if (callExpression == null)
            {
                return;
            }

            foreach (var referenceExpression in callExpression.Descendants<IReferenceExpression>())
            {
                var nameIdentifier = referenceExpression.NameIdentifier;
                if (nameIdentifier != null && nameIdentifier.Name == methodName)
                {
                    AddNameRange(ranges, nameIdentifier);

                    return;
                }
            }
        }

        private static void AddNameRange([NotNull] ICollection<DocumentRange> ranges, [CanBeNull] ITreeNode identifier)
        {
            var range = identifier?.GetDocumentRange() ?? DocumentRange.InvalidRange;
            if (range.IsValid())
            {
                ranges.Add(range);
            }
        }

        /// <summary>
        /// The innermost convertible call around the caret. Every enclosing call is tried, because the caret
        /// may well sit inside an argument that is itself a call.
        /// </summary>
        [CanBeNull]
        private LoggerMessageCallModel TryBuildModel()
        {
            var selectedInvocation = _provider.GetSelectedTreeNode<IInvocationExpression>();
            if (selectedInvocation == null)
            {
                return null;
            }

            var messageTemplateParser = _provider.Solution.GetComponent<MessageTemplateParser>();
            var templateParameterNameAttributeProvider = _provider.PsiServices.GetCodeAnnotationsCache()
                .GetProvider<TemplateParameterNameAttributeProvider>();

            for (var invocationExpression = selectedInvocation;
                 invocationExpression != null;
                 invocationExpression = invocationExpression.GetContainingNode<IInvocationExpression>())
            {
                var model = LoggerMessageCallModel.TryBuild(
                    invocationExpression,
                    templateParameterNameAttributeProvider,
                    messageTemplateParser);
                if (model != null)
                {
                    return model;
                }
            }

            return null;
        }
    }
}
