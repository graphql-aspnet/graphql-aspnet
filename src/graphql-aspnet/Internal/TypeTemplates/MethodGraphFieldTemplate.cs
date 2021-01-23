// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.Resolvers;

    /// <summary>
    /// A base class representing common items between all <see cref="IGraphType"/> capable
    /// methods.
    /// </summary>
    [DebuggerDisplay("Route: {Route.Path}")]
    public abstract class MethodGraphFieldTemplate : GraphTypeFieldTemplate, IGraphMethod
    {
        private readonly List<GraphFieldArgumentTemplate> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodGraphFieldTemplate" /> class.
        /// </summary>
        /// <param name="parent">The parent object template that owns this method.</param>
        /// <param name="methodInfo">The method information.</param>
        protected MethodGraphFieldTemplate(IGraphTypeTemplate parent, MethodInfo methodInfo)
            : base(parent, methodInfo)
        {
            this.Method = Validation.ThrowIfNullOrReturn(methodInfo, nameof(methodInfo));
            _arguments = new List<GraphFieldArgumentTemplate>();
        }

        /// <summary>
        /// When overridden in a child class this method builds out the template according to its own individual requirements.
        /// </summary>
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            // parse all input parameters from the method signature
            foreach (var parameter in this.Method.GetParameters())
            {
                var argTemplate = this.CreateGraphFieldArgument(parameter);
                argTemplate.Parse();
                _arguments.Add(argTemplate);
            }

            this.ExpectedReturnType = GraphValidation.EliminateWrappersFromCoreType(
                this.DeclaredReturnType,
                false,
                true,
                false);
        }

        /// <summary>
        /// Creates graph field argument for this template given the parameter info supplied.
        /// </summary>
        /// <param name="paramInfo">The parameter information.</param>
        /// <returns>IGraphFieldArgumentTemplate.</returns>
        protected virtual GraphFieldArgumentTemplate CreateGraphFieldArgument(ParameterInfo paramInfo)
        {
            return new GraphFieldArgumentTemplate(this, paramInfo);
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            if (this.Method.IsStatic)
            {
                throw new GraphTypeDeclarationException(
                    $"Invalid graph method declaration. The method '{this.InternalFullName}' is static. Only " +
                    "instance members can be registered as field.");
            }

            if (this.ExpectedReturnType == null)
            {
                throw new GraphTypeDeclarationException(
                   $"Invalid graph method declaration. The method '{this.InternalFullName}' has no valid {nameof(ExpectedReturnType)}. An expected " +
                   "return type must be assigned from the declared return type.");
            }
        }

        /// <summary>
        /// Creates a resolver capable of resolving this field.
        /// </summary>
        /// <returns>IGraphFieldResolver.</returns>
        public override IGraphFieldResolver CreateResolver()
        {
            return new GraphObjectMethodResolver(this);
        }

        /// <summary>
        /// Gets the fully qualified name, including namespace, of this item as it exists in the .NET code (e.g. 'Namespace.ObjectType.MethodName').
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public override string InternalFullName => $"{this.Parent?.InternalFullName}.{this.Method.Name}";

        /// <summary>
        /// Gets the name that defines this item within the .NET code of the application; typically a method name or property name.
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public override string InternalName => this.Method.Name;

        /// <summary>
        /// Gets a list of parameters, in the order they are declared on this field.
        /// </summary>
        /// <value>The parameters.</value>
        public override IReadOnlyList<IGraphFieldArgumentTemplate> Arguments => _arguments;

        /// <summary>
        /// Gets method meta data this method template applies to.
        /// </summary>
        /// <value>The controller method.</value>
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets the return type of this field as its declared in the C# code base with no modifications or
        /// coerions applied.
        /// </summary>
        /// <value>The type naturally returned by this field.</value>
        public override Type DeclaredReturnType => this.Method.ReturnType;

        /// <summary>
        /// Gets the type, unwrapped of any tasks, that this graph method should return upon completion. This value
        /// represents the implementation return type as opposed to the expected graph type.
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ExpectedReturnType { get; private set; }

        /// <summary>
        /// Gets the name this field is declared as in the C# code (method name or property name).
        /// </summary>
        /// <value>The name of the declared.</value>
        public override string DeclaredName => this.Method.Name;

        /// <summary>
        /// Gets the source type this field was created from.
        /// </summary>
        /// <value>The field souce.</value>
        public override GraphFieldTemplateSource FieldSource => GraphFieldTemplateSource.Method;
    }
}