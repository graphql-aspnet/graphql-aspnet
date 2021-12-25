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
        public GraphDirectiveMethodTemplate(IGraphTypeTemplate parent, MethodInfo method)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            this.Method = Validation.ThrowIfNullOrReturn(method, nameof(method));
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

            this.IsValidDirectiveMethod = (this.Method.ReturnType == typeof(IGraphActionResult) || this.Method.ReturnType == typeof(Task<IGraphActionResult>)) &&
                                          !this.Method.IsGenericMethod;

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

            // extract the lifecycle point indicated by this method
            DirectiveLifeCyclePhase lifeCycle = DirectiveLifeCyclePhase.None;
            switch (this.Method.Name)
            {
                case Constants.ReservedNames.DIRECTIVE_BEFORE_RESOLUTION_METHOD_NAME:
                    lifeCycle = DirectiveLifeCyclePhase.BeforeResolution;
                    break;

                case Constants.ReservedNames.DIRECTIVE_AFTER_RESOLUTION_METHOD_NAME:
                    lifeCycle = DirectiveLifeCyclePhase.AfterResolution;
                    break;

                case Constants.ReservedNames.DIRECTIVE_ALTER_TYPE_SYSTEM_METHOD_NAME:
                    lifeCycle = DirectiveLifeCyclePhase.AlterTypeSystem;
                    break;
            }

            this.LifeCycle = lifeCycle;
            this.Route = new GraphFieldPath(GraphFieldPath.Join(this.Parent.Route.Path, this.LifeCycle.ToString()));

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
        /// A factor method to creates a qualified method template for the given <paramref name="methodInfo"/>.
        /// </summary>
        /// <param name="parentTemplate">The template that owns <paramref name="methodInfo"/>.</param>
        /// <param name="methodInfo">The method information to parse and create a template from.</param>
        /// <returns>A completed method template or <c>null</c>.</returns>
        public static GraphDirectiveMethodTemplate CreateMethodTemplate(IGraphTypeTemplate parentTemplate, MethodInfo methodInfo)
        {
            if (methodInfo != null && methodInfo.SingleAttributeOrDefault<GraphSkipAttribute>() == null)
            {
                switch (methodInfo.Name)
                {
                    case Constants.ReservedNames.DIRECTIVE_BEFORE_RESOLUTION_METHOD_NAME:
                    case Constants.ReservedNames.DIRECTIVE_AFTER_RESOLUTION_METHOD_NAME:
                    case Constants.ReservedNames.DIRECTIVE_ALTER_TYPE_SYSTEM_METHOD_NAME:
                        return new GraphDirectiveMethodTemplate(parentTemplate, methodInfo);
                }
            }

            return null;
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

            if (!this.IsValidDirectiveMethod)
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

            if (this.LifeCycle.IsTypeSystemPhase())
            {
                if (_arguments.Count != 1 || _arguments[1].ObjectType != typeof(ISchemaItem))
                {
                    throw new GraphTypeDeclarationException(
                       $"The directive method '{this.InternalFullName}' must declare exactly one input parameter of type '{nameof(ISchemaItem)}'.");
                }
            }
        }

        /// <summary>
        /// Gets the life cycle method hook this instance is representing.
        /// </summary>
        /// <value>The life cycle method.</value>
        public DirectiveLifeCyclePhase LifeCycle { get; private set; }

        /// <summary>
        /// Gets the actual type wrappers used to generate a type expression for this field.
        /// This list represents the type requirements  of the field.
        /// </summary>
        /// <value>The custom wrappers.</value>
        public MetaGraphTypes[] TypeWrappers => null;

        /// <summary>
        /// Gets declared type of item minus any asyncronous wrappers (i.e. the T in Task{T}).
        /// </summary>
        /// <value>The type of the declared.</value>
        public Type DeclaredType { get; private set; }

        /// <summary>
        /// Gets the type expression that represents the data returned from this field (i.e. the '[SomeType!]'
        /// declaration used in schema definition language.)
        /// </summary>
        /// <value>The type expression.</value>
        public GraphTypeExpression TypeExpression { get; private set; }

        /// <summary>
        /// Gets the human readable method signature identifying this method.
        /// </summary>
        /// <value>The method signature.</value>
        public string MethodSignature { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has a defined default value.
        /// </summary>
        /// <value><c>true</c> if this instance has a default value; otherwise, <c>false</c>.</value>
        public bool HasDefaultValue => false;

        /// <summary>
        /// Gets the human-readable description distributed with this field
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        public string Description { get; private set; }

        /// <summary>
        /// Gets a the canonical path on the graph where this item sits.
        /// </summary>
        /// <value>The route.</value>
        public GraphFieldPath Route { get; private set; }

        /// <summary>
        /// Gets a list of parameters, in the order they are declared
        /// that are part of this method.
        /// </summary>
        /// <value>The parameters.</value>
        public IReadOnlyList<IGraphFieldArgumentTemplate> Arguments => _arguments;

        /// <summary>
        /// Gets or sets the type, unwrapped of any tasks, that this graph method should return upon completion. This value
        /// represents the implementation return type as opposed to the expected graph type.
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ExpectedReturnType { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is asynchronous method.
        /// </summary>
        /// <value><c>true</c> if this instance is asynchronous method; otherwise, <c>false</c>.</value>
        public bool IsAsyncField { get; protected set; }

        /// <summary>
        /// Gets or sets the concrete type this template represents.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance was explictly declared as a graph item via acceptable attribution or
        /// if it was parsed as a matter of completeness.
        /// </summary>
        /// <value><c>true</c> if this instance is explictly declared; otherwise, <c>false</c>.</value>
        public bool IsExplicitDeclaration => true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is valid directive method.
        /// </summary>
        /// <value><c>true</c> if this instance is valid directive method; otherwise, <c>false</c>.</value>
        public bool IsValidDirectiveMethod { get; protected set; }

        /// <summary>
        /// Gets the name of the item on the object graph as it is conveyed in an introspection request.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this.Method.Name;

        /// <summary>
        /// Gets the type of the source object.
        /// </summary>
        /// <value>The type of the source object.</value>
        public Type SourceObjectType => this.Parent?.ObjectType;

        /// <summary>
        /// Gets the parent directive template this instance belongs to.
        /// </summary>
        /// <value>The parent.</value>
        public IGraphTypeTemplate Parent { get; }

        /// <summary>
        /// Gets the method information this instance describes.
        /// </summary>
        /// <value>The method.</value>
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets the source type this field was created from.
        /// </summary>
        /// <value>The field souce.</value>
        public GraphFieldSource FieldSource => GraphFieldSource.Method;

        /// <summary>
        /// Gets the fully qualified name, including namespace, of this item as it exists in the .NET code (e.g. 'Namespace.ObjectType.MethodName').
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public string InternalFullName => $"{this.Parent?.InternalFullName}.{this.Method.Name}";

        /// <summary>
        /// Gets the name that defines this item within the .NET code of the application; typically a method name or property name.
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public string InternalName => this.Method.Name;
    }
}