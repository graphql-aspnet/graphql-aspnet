// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeTemplates
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryOperationSteps;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A template describing an action method declared on a directive.
    /// </summary>
    [DebuggerDisplay("{InternalName} (Type: {Parent.InternalName})")]
    public class GraphDirectiveMethodTemplate : IGraphFieldTemplateBase
    {
        private readonly List<GraphArgumentTemplate> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveMethodTemplate"/> class.
        /// </summary>
        /// <param name="parent">The owner of this method.</param>
        /// <param name="method">The method information.</param>
        public GraphDirectiveMethodTemplate(IGraphTypeTemplate parent, MethodInfo method)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            this.Method = Validation.ThrowIfNullOrReturn(method, nameof(method));
            this.Parameters = this.Method.GetParameters().ToList();
            _arguments = new List<GraphArgumentTemplate>();
            this.AttributeProvider = this.Method;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveMethodTemplate" /> class.
        /// </summary>
        /// <param name="parent">The owner of this method.</param>
        /// <param name="method">The method information.</param>
        /// <param name="attributeProvider">A custom attribute provider this template will use
        /// to perform its configuration.</param>
        public GraphDirectiveMethodTemplate(IGraphTypeTemplate parent, MethodInfo method, ICustomAttributeProvider attributeProvider)
            : this(parent, method)
        {
            this.AttributeProvider = Validation.ThrowIfNullOrReturn(attributeProvider, nameof(attributeProvider));
        }

        /// <summary>
        /// Parses this template to extract required meta data about the directive method.
        /// </summary>
        public virtual void Parse()
        {
            this.DeclaredType = this.Method.ReturnType;
            this.ObjectType = GraphValidation.EliminateWrappersFromCoreType(this.DeclaredType);
            this.TypeExpression = new GraphTypeExpression(this.ObjectType.FriendlyName());

            this.Description = this.AttributeProvider.SingleAttributeOrDefault<DescriptionAttribute>()?.Description;
            this.IsAsyncField = Validation.IsCastable<Task>(this.Method.ReturnType);
            this.AppliedDirectives = this.ExtractAppliedDirectiveTemplates();
            this.InternalName = $"{this.Parent?.InternalName ?? "UnknownDirective"}.{this.Method.Name}";

            // deteremine all the directive locations where this method should be invoked
            var locations = DirectiveLocation.NONE;
            foreach (var attrib in this.AttributeProvider.AttributesOfType<DirectiveLocationsAttribute>())
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

            this.Route = new SchemaItemPath(SchemaItemPath.Join(this.Parent.Route.Path, this.Name));

            // parse all input parameters into the method
            foreach (var parameter in this.Method.GetParameters())
            {
                var argTemplate = new GraphArgumentTemplate(this, parameter);
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
                    $"The directive resolver {this.InternalName} defines a {nameof(GraphSkipAttribute)}. It cannot be parsed or added " +
                    "to the object graph.");
            }

            if (this.AppliedDirectives.Any())
            {
                throw new GraphTypeDeclarationException(
                    $"The directive resolver {this.InternalName} defines an {nameof(ApplyDirectiveAttribute)}. " +
                    $"Directive methods cannot have applied directives.");
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
                        $"The directive resolver '{this.InternalName}' defines a return type of'{typeof(Task).Name}' but " +
                        "defines no contained return type for the resultant model object yielding a void return after " +
                        "completion of the task. All resolvers must return a single model object. Consider using " +
                        $"'{typeof(Task<>).Name}' instead for asyncronous methods");
                }
            }

            if (this.ExpectedReturnType != typeof(IGraphActionResult))
            {
                throw new GraphTypeDeclarationException(
                    $"The directive resolver '{this.InternalName}' does not return a {nameof(IGraphActionResult)}. " +
                    $"All directive resolvers must return a {nameof(IGraphActionResult)} or {typeof(Task<IGraphActionResult>).FriendlyName()}");
            }

            if (string.IsNullOrWhiteSpace(this.InternalName))
            {
                throw new GraphTypeDeclarationException(
                    $"The directive resolver template identified by `{this.Method.Name}` does not declare a valid internal name.");
            }

            foreach (var argument in _arguments)
                argument.ValidateOrThrow();

            foreach (var directive in this.AppliedDirectives)
                directive.ValidateOrThrow();
        }

        /// <inheritdoc />
        public IEnumerable<DependentType> RetrieveRequiredTypes()
        {
            var list = new List<DependentType>();
            foreach (var argument in this.Arguments)
                list.AddRange(argument.RetrieveRequiredTypes());

            if (this.AppliedDirectives != null)
            {
                var dependentTypes = this.AppliedDirectives
                    .Where(x => x.DirectiveType != null)
                    .Select(x => new DependentType(x.DirectiveType, TypeKind.DIRECTIVE));
                list.AddRange(dependentTypes);
            }

            return list;
        }

        /// <inheritdoc cref="IGraphArgumentTemplate.CreateResolverMetaData" />
        public IGraphFieldResolverMetaData CreateResolverMetaData()
        {
            var paramSet = new FieldResolverParameterMetaDataCollection(
                this.Arguments.Select(x => x.CreateResolverMetaData()));

            return new FieldResolverMetaData(
                this.Method,
                paramSet,
                this.ExpectedReturnType,
                this.IsAsyncField,
                this.InternalName,
                this.Method.Name,
                this.Parent.ObjectType,
                this.Parent.InternalName,
                this.Parent.TemplateSource);
        }

        /// <summary>
        /// Gets the bitwise flags of locations that this method is defined
        /// to handle.
        /// </summary>
        /// <value>The locations.</value>
        public DirectiveLocation Locations { get; private set; }

        /// <inheritdoc />
        public MetaGraphTypes[] DeclaredTypeWrappers => null; // not used by directives

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
        public string Description { get; private set; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<IGraphArgumentTemplate> Arguments => _arguments;

        /// <inheritdoc cref="IGraphFieldResolverMetaData.ExpectedReturnType" />
        public Type ExpectedReturnType { get; protected set; }

        /// <inheritdoc cref="IGraphFieldResolverMetaData.IsAsyncField" />
        public bool IsAsyncField { get; protected set; }

        /// <inheritdoc />
        public Type ObjectType { get; protected set; }

        /// <inheritdoc />
        public bool IsExplicitDeclaration => true;

        /// <inheritdoc />
        public string Name => this.Method.Name;

        /// <inheritdoc />
        public Type SourceObjectType => this.Parent?.ObjectType;

        /// <inheritdoc cref="IGraphFieldTemplate.Parent" />
        public IGraphTypeTemplate Parent { get; }

        /// <inheritdoc cref="IGraphFieldResolverMetaData.Method" />
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets the set of parameters declared on the <see cref="Method"/>.
        /// </summary>
        /// <value>The parameters.</value>
        public IReadOnlyList<ParameterInfo> Parameters { get; }

        /// <inheritdoc />
        public GraphFieldSource FieldSource => GraphFieldSource.Method;

        /// <inheritdoc />
        public string InternalName { get; protected set; }

        /// <inheritdoc />
        public ICustomAttributeProvider AttributeProvider { get; }

        /// <inheritdoc />
        public IEnumerable<IAppliedDirectiveTemplate> AppliedDirectives { get; private set; }
    }
}