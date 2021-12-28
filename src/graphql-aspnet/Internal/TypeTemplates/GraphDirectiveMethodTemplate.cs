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
        private readonly List<GraphInputArgumentTemplate> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveMethodTemplate"/> class.
        /// </summary>
        /// <param name="parent">The owner of this method.</param>
        /// <param name="method">The method information.</param>
        internal GraphDirectiveMethodTemplate(IGraphTypeTemplate parent, MethodInfo method)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            this.Method = Validation.ThrowIfNullOrReturn(method, nameof(method));
            _arguments = new List<GraphInputArgumentTemplate>();
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

            // deteremine all the directive locations where this method should be invoked
            var locations = DirectiveLocation.NONE;
            foreach (var attrib in this.Method.AttributesOfType<DirectiveLocationsAttribute>())
            {
                locations = locations | attrib.Locations;
            }

            this.Locations = locations;

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

            this.Route = new GraphFieldPath(GraphFieldPath.Join(this.Parent.Route.Path, this.Name));

            // parse all input parameters into the method
            foreach (var parameter in this.Method.GetParameters())
            {
                var argTemplate = new GraphInputArgumentTemplate(this, parameter);
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

            if (this.ExpectedReturnType != typeof(IGraphActionResult))
            {
                throw new GraphTypeDeclarationException(
                    $"The directive method '{this.InternalFullName}' does not return a {nameof(IGraphActionResult)}. " +
                    $"All directive methods must return a {nameof(IGraphActionResult)} or {typeof(Task<IGraphActionResult>).FriendlyName()}");
            }

            foreach (var argument in _arguments)
                argument.ValidateOrThrow();
        }

        /// <summary>
        /// Gets the bitwise flags of locations that this method is defined
        /// to handle.
        /// </summary>
        /// <value>The locations.</value>
        public DirectiveLocation Locations { get; private set; }

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
        public IReadOnlyList<IGraphInputArgumentTemplate> Arguments => _arguments;

        /// <inheritdoc />
        public Type ExpectedReturnType { get; protected set; }

        /// <inheritdoc />
        public bool IsAsyncField { get; protected set; }

        /// <inheritdoc />
        public Type ObjectType { get; protected set; }

        /// <inheritdoc />
        public bool IsExplicitDeclaration => true;

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