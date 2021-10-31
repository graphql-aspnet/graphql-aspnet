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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A parsed description of the meta data of any given property that should be represented
    /// as a field on a type in an <see cref="ISchema" />.
    /// </summary>
    [DebuggerDisplay("Route: {Route.Path}")]
    public class PropertyGraphFieldTemplate : GraphFieldTemplate, IGraphMethod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGraphFieldTemplate" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="propInfo">The property information.</param>
        /// <param name="ownerKind">The kind of graph type that will own this field.</param>
        public PropertyGraphFieldTemplate(IGraphTypeTemplate parent, PropertyInfo propInfo, TypeKind ownerKind)
            : base(parent, propInfo)
        {
            this.Property = Validation.ThrowIfNullOrReturn(propInfo, nameof(propInfo));
            this.OwnerTypeKind = ownerKind;
        }

        /// <summary>
        /// When overridden in a child class, this method builds the route that will be assigned to this method
        /// using the implementation rules of the concrete type.
        /// </summary>
        /// <returns>GraphRoutePath.</returns>
        protected override GraphFieldPath GenerateFieldPath()
        {
            // A class property cannot contain any route pathing or nesting like controllers or actions.
            // Before creating hte route, ensure that the declared name, by itself, is valid for graphql such that resultant
            // global path for this property will also be correct.
            var graphName = this.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>()?.Template?.Trim() ?? Constants.Routing.ACTION_METHOD_META_NAME;
            graphName = graphName.Replace(Constants.Routing.ACTION_METHOD_META_NAME, this.Property.Name).Trim();

            return new GraphFieldPath(GraphFieldPath.Join(this.Parent.Route.Path, graphName));
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            // ensure property has a public getter (kinda useless otherwise)
            if (this.Property.GetGetMethod() == null)
            {
                throw new GraphTypeDeclarationException(
                    $"The graph property {this.InternalFullName} does not define a public getter.  It cannot be parsed or added " +
                    "to the object graph.");
            }

            if (this.ExpectedReturnType == null)
            {
                throw new GraphTypeDeclarationException(
                    $"The graph property '{this.InternalFullName}' has no valid {nameof(ExpectedReturnType)}. An expected " +
                    "return type must be assigned from the declared return type.");
            }
        }

        /// <summary>
        /// When overridden in a child class this method builds out the template according to its own individual requirements.
        /// </summary>
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            this.ExpectedReturnType = GraphValidation.EliminateWrappersFromCoreType(
                this.DeclaredReturnType,
                false,
                true,
                false);
        }

        /// <summary>
        /// Creates a resolver capable of resolving this field.
        /// </summary>
        /// <returns>IGraphFieldResolver.</returns>
        public override IGraphFieldResolver CreateResolver()
        {
            return new GraphObjectPropertyResolver(this);
        }

        /// <summary>
        /// Gets the actual declared return type of this field before any manipulation has been made to it. For properties and methods
        /// this should be the actual type returned from the method or property.
        /// </summary>
        /// <value>The type of the declared return.</value>
        public override Type DeclaredReturnType => this.Property.PropertyType;

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
        public override string DeclaredName => this.Property.Name;

        /// <summary>
        /// Gets the source type this field was created from.
        /// </summary>
        /// <value>The field souce.</value>
        public override GraphFieldSource FieldSource => GraphFieldSource.Property;

        /// <summary>
        /// Gets the kind of graph type that should own fields created from this template.
        /// </summary>
        /// <value>The kind.</value>
        public override TypeKind OwnerTypeKind { get; }

        /// <summary>
        /// Gets the core property information about this template.
        /// </summary>
        /// <value>The property.</value>
        private PropertyInfo Property { get; }

        /// <summary>
        /// Gets the action method to be called.
        /// </summary>
        /// <value>The action method.</value>
        public MethodInfo Method => this.Property.GetGetMethod();

        /// <summary>
        /// Gets a list of parameters, in the order they are declared on this field.
        /// </summary>
        /// <value>The parameters.</value>
        public override IReadOnlyList<IGraphFieldArgumentTemplate> Arguments { get; } = new List<IGraphFieldArgumentTemplate>();

        /// <summary>
        /// Gets the fully qualified name, including namespace, of this item as it exists in the .NET code (e.g. 'Namespace.ObjectType.MethodName').
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public override string InternalFullName => $"{this.Parent.InternalFullName}.{this.Property.Name}";

        /// <summary>
        /// Gets the name that defines this item within the .NET code of the application; typically a method name or property name.
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public override string InternalName => this.Property.Name;
    }
}