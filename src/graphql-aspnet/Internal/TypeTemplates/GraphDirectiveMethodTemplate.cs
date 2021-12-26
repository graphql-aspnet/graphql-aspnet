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
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base template for all directive methods that can be declared.
    /// </summary>
    public class GraphDirectiveMethodTemplate : IGraphFieldBaseTemplate, IGraphMethod
    {
        private readonly List<GraphFieldArgumentTemplate> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveMethodTemplate"/> class.
        /// </summary>
        /// <param name="parent">The owner of this method.</param>
        /// <param name="method">The method information.</param>
        internal GraphDirectiveMethodTemplate(IGraphTypeTemplate parent, MethodInfo method)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            this.Method = Validation.ThrowIfNullOrReturn(method, nameof(method));
            this.LifeCyclePhase = DirectiveLifeCyclePhase.Unknown;
            _arguments = new List<GraphFieldArgumentTemplate>();
        }

        /// <summary>
        /// Parses this template to extract required meta data about the directive method.
        /// </summary>
        public virtual void Parse()
        {
            this.DeclaredType = this.Method.ReturnType;
            this.ObjectType = GraphValidation.EliminateWrappersFromCoreType(this.DeclaredType);
            this.TypeExpression = new GraphTypeExpression(this.ObjectType.FriendlyName());

            this.Description = this.Method.SingleAttributeOrDefault<DescriptionAttribute>()?.Description;
            this.IsAsyncField = Validation.IsCastable<Task>(this.Method.ReturnType);

            this.IsValidDirectiveMethodSignature = (this.Method.ReturnType == typeof(IGraphActionResult) || this.Method.ReturnType == typeof(Task<IGraphActionResult>)) &&
                                          !this.Method.IsGenericMethod;

            // extract the lifecycle point indicated by this method
            if (Constants.ReservedNames.DirectiveLifeCycleMethodNames.ContainsKey(this.Method.Name))
            {
                this.IsValidDirectiveMethodName = true;
                this.LifeCyclePhase = Constants.ReservedNames.DirectiveLifeCycleMethodNames[this.Method.Name];
            }

            // is the method asyncronous? if so ensure that a Task<T> is returned
            // and not an empty task
            if (this.IsAsyncField && this.Method.ReturnType.IsGenericType)
            {
                // for any ssync field attempt to pull out the T in Task<T>
                var genericArgs = this.Method.ReturnType.GetGenericArguments();
                if (genericArgs.Any())
                    this.DeclaredType = genericArgs[0];
            }

            this.ExpectedReturnType = GraphValidation.EliminateWrappersFromCoreType(
                this.DeclaredType,
                false,
                true,
                false);

            this.Route = new GraphFieldPath(GraphFieldPath.Join(this.Parent.Route.Path, this.LifeCyclePhase.ToString()));

            // parse all input parameters into the method
            foreach (var parameter in this.Method.GetParameters())
            {
                var argTemplate = new GraphFieldArgumentTemplate(this, parameter);
                argTemplate.Parse();
                _arguments.Add(argTemplate);
            }

            this.MethodSignature = this.GenerateMethodSignatureString();
        }

        /// <summary>
        /// Generates a human readable method signature string to use in error reporting.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GenerateMethodSignatureString()
        {
            // e.g.   "Task<int> (int, string, SomeType)"
            var builder = new StringBuilder();
            builder.Append($"{this.ObjectType.FriendlyName()} (");
            for (var i = 0; i < _arguments.Count; i++)
            {
                var parameter = _arguments[i];
                builder.Append($"{parameter.ObjectType?.FriendlyName()} {parameter.Name}");

                if (i < _arguments.Count - 1)
                    builder.Append(", ");
            }

            builder.Append(")");
            return builder.ToString();
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public virtual void ValidateOrThrow()
        {
            // ensure skip isnt set
            if (this.Method.SingleAttributeOrDefault<GraphSkipAttribute>() != null)
            {
                throw new GraphTypeDeclarationException(
                    $"The graph method {this.InternalFullName} defines a {nameof(GraphSkipAttribute)}. It cannot be parsed or added " +
                    "to the object graph.");
            }

            if (this.LifeCyclePhase == DirectiveLifeCyclePhase.Unknown)
            {
                throw new GraphTypeDeclarationException(
                    $"The directive method '{this.InternalFullName}' has does not have a valid lifecycle phase.");
            }

            // is the method asyncronous? if so ensure that a Task<T> is returned
            // and not an empty task
            if (this.IsAsyncField)
            {
                // if the return is just Task (not Task<T>) then error out
                var genericArgs = this.Method.ReturnType.GetGenericArguments();
                if (genericArgs.Length != 1)
                {
                    throw new GraphTypeDeclarationException(
                        $"The directive method '{this.InternalFullName}' defines a return type of'{typeof(Task).Name}' but " +
                        "defines no contained return type for the resultant model object yielding a void return after " +
                        "completion of the task. All graph methods must return a single model object. Consider using " +
                        $"'{typeof(Task<>).Name}' instead for asyncronous methods");
                }
            }

            if (!this.IsValidDirectiveMethodName)
            {
                var allowedNames = string.Join(", ", Constants.ReservedNames.DirectiveLifeCycleMethodNames.Keys);
                throw new GraphTypeDeclarationException(
                    $"The method '{this.InternalFullName}' is not allowed lifecycle method name. " +
                    $"All Directive methods must be one of the known lifecycle method names. ({allowedNames}).");
            }

            if (!this.IsValidDirectiveMethodSignature)
            {
                throw new GraphTypeDeclarationException(
                    $"The method '{this.InternalFullName}' has an invalid signature and cannot be used as a directive " +
                    $"method. All Directive methods must not contain generic parameters and must return a '{typeof(IGraphActionResult).FriendlyName()}' " +
                    $"to be invoked properly.");
            }

            if (this.ExpectedReturnType == null)
            {
                throw new GraphTypeDeclarationException(
                   $"The directive method '{this.InternalFullName}' has no valid {nameof(ExpectedReturnType)}. An expected " +
                   "return type must be assigned from the declared return type.");
            }

            foreach (var argument in _arguments)
                argument.ValidateOrThrow();

            if (this.LifeCyclePhase.IsTypeSystemPhase())
            {
                if (_arguments.Count != 1 || _arguments[1].ObjectType != typeof(ISchemaItem))
                {
                    throw new GraphTypeDeclarationException(
                       $"The directive method '{this.InternalFullName}' must declare exactly one input parameter of type '{nameof(ISchemaItem)}'.");
                }
            }
        }

        /// <summary>
        /// Gets the life cycle phase this method represents within its parent directive.
        /// </summary>
        /// <value>The life cycle phase.</value>
        public DirectiveLifeCyclePhase LifeCyclePhase { get; private set; }

        /// <inheritdoc />
        public MetaGraphTypes[] TypeWrappers => null; // not used by directives

        /// <summary>
        /// Gets declared return type of the method minus any asyncronous wrappers (i.e. the T in Task{T}).
        /// </summary>
        /// <value>The type of the declared.</value>
        public Type DeclaredType { get; private set; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; private set; }

        /// <summary>
        /// Gets the defining method signature of this template, its return type and expected parameters. (e.g. <c>string (int var1)</c>).
        /// </summary>
        /// <value>The method signature.</value>
        public string MethodSignature { get; private set; }

        /// <inheritdoc />
        public bool HasDefaultValue => false;

        /// <inheritdoc />
        public string Description { get; private set; }

        /// <inheritdoc />
        public GraphFieldPath Route { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<IGraphFieldArgumentTemplate> Arguments => _arguments;

        /// <inheritdoc />
        public Type ExpectedReturnType { get; protected set; }

        /// <inheritdoc />
        public bool IsAsyncField { get; protected set; }

        /// <inheritdoc />
        public Type ObjectType { get; protected set; }

        /// <inheritdoc />
        public bool IsExplicitDeclaration => true;

        /// <summary>
        /// Gets a value indicating whether this method is a valid directive method.
        /// </summary>
        /// <value><c>true</c> if this instance is valid directive method; otherwise, <c>false</c>.</value>
        public bool IsValidDirectiveMethodSignature { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this method matches one of the allowed method names.
        /// </summary>
        /// <value><c>true</c> if this instance is valid directive method name; otherwise, <c>false</c>.</value>
        public bool IsValidDirectiveMethodName { get; private set; }

        /// <inheritdoc />
        public string Name => this.Method.Name;

        /// <inheritdoc />
        public Type SourceObjectType => this.Parent?.ObjectType;

        /// <inheritdoc />
        public IGraphTypeTemplate Parent { get; }

        /// <inheritdoc />
        public MethodInfo Method { get; }

        /// <inheritdoc />
        public GraphFieldSource FieldSource => GraphFieldSource.Method;

        /// <inheritdoc />
        public string InternalFullName => $"{this.Parent?.InternalFullName}.{this.Method.Name}";

        /// <inheritdoc />
        public string InternalName => this.Method.Name;
    }
}