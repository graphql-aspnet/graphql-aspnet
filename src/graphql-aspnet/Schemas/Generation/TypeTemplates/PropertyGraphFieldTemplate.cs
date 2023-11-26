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
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An template describing a field on an OBJECT or INTERFACE graph type that
    /// is created from a C# object property.
    /// </summary>
    [DebuggerDisplay("Route: {Route.Path}")]
    public class PropertyGraphFieldTemplate : GraphFieldTemplateBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGraphFieldTemplate" /> class.
        /// </summary>
        /// <param name="parent">The owner of this field template.</param>
        /// <param name="propInfo">The property information that will be used to create the field.</param>
        /// <param name="attributeProvider">A custom, external attribute provider to use instead for extracting
        /// configuration attributes instead of the provider on <paramref name="propInfo"/>.</param>
        /// <param name="ownerKind">The kind of graph type that will own this field.</param>
        public PropertyGraphFieldTemplate(IGraphTypeTemplate parent, PropertyInfo propInfo, ICustomAttributeProvider attributeProvider, TypeKind ownerKind)
            : base(parent, attributeProvider)
        {
            this.Property = Validation.ThrowIfNullOrReturn(propInfo, nameof(propInfo));
            this.Method = this.Property.GetGetMethod();
            this.Parameters = this.Method?.GetParameters().ToList() ?? new List<ParameterInfo>();
            this.OwnerTypeKind = ownerKind;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGraphFieldTemplate" /> class.
        /// </summary>
        /// <param name="parent">The owner of this field template.</param>
        /// <param name="propInfo">The property information that will be used to create the field.</param>
        /// <param name="ownerKind">The kind of graph type that will own this field.</param>
        public PropertyGraphFieldTemplate(IGraphTypeTemplate parent, PropertyInfo propInfo, TypeKind ownerKind)
            : base(parent, propInfo)
        {
            this.Property = Validation.ThrowIfNullOrReturn(propInfo, nameof(propInfo));
            this.Method = this.Property.GetGetMethod();
            this.Parameters = this.Method?.GetParameters().ToList() ?? new List<ParameterInfo>();
            this.OwnerTypeKind = ownerKind;
        }

        /// <inheritdoc />
        protected override SchemaItemPath GenerateFieldPath()
        {
            // A class property cannot contain any route pathing or nesting like controllers or actions.
            // Before creating hte route, ensure that the declared name, by itself, is valid for graphql such that resultant
            // global path for this property will also be correct.
            var graphName = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>()?.Template?.Trim() ?? Constants.Routing.ACTION_METHOD_META_NAME;
            graphName = graphName.Replace(Constants.Routing.ACTION_METHOD_META_NAME, this.Property.Name).Trim();

            return new SchemaItemPath(SchemaItemPath.Join(this.Parent.Route.Path, graphName));
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            // ensure property has a public getter (kinda useless otherwise)
            if (this.Property.GetGetMethod() == null)
            {
                throw new GraphTypeDeclarationException(
                    $"The graph property {this.InternalName} does not define a public getter.  It cannot be parsed or added " +
                    "to the object graph.");
            }

            if (this.ExpectedReturnType == null)
            {
                throw new GraphTypeDeclarationException(
                    $"The graph property '{this.InternalName}' has no valid {nameof(this.ExpectedReturnType)}. An expected " +
                    "return type must be assigned from the declared return type.");
            }

            base.ValidateOrThrow(validateChildren);
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            // set the internal name of the item
            var fieldDeclaration = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>();
            if (fieldDeclaration != null)
                this.InternalName = fieldDeclaration.InternalName;
            if (string.IsNullOrWhiteSpace(this.InternalName) && this.Method != null)
                this.InternalName = $"{this.Parent.InternalName}.{this.Method.Name}";

            this.ExpectedReturnType = GraphValidation.EliminateWrappersFromCoreType(
                this.DeclaredReturnType,
                false,
                true,
                false);
        }

        /// <inheritdoc />
        public override IGraphFieldResolver CreateResolver()
        {
            return new ObjectPropertyGraphFieldResolver(this.CreateResolverMetaData());
        }

        /// <inheritdoc />
        public override IGraphFieldResolverMetaData CreateResolverMetaData()
        {
            var paramSet = new FieldResolverParameterMetaDataCollection(
                this.Arguments.Select(x => x.CreateResolverMetaData()));

            return new FieldResolverMetaData(
                this.Method,
                paramSet,
                this.ExpectedReturnType,
                this.IsAsyncField,
                this.InternalName,
                this.Property.Name,
                this.Parent.ObjectType,
                this.Parent.InternalName,
                this.Parent.TemplateSource);
        }

        /// <inheritdoc />
        public override Type DeclaredReturnType => this.Property.PropertyType;

        /// <inheritdoc cref="IGraphFieldResolverMetaData.ExpectedReturnType" />
        public Type ExpectedReturnType { get; private set; }

        /// <inheritdoc />
        public override string DeclaredName => this.Property.Name;

        /// <inheritdoc />
        public override GraphFieldSource FieldSource => GraphFieldSource.Property;

        /// <inheritdoc />
        public override TypeKind OwnerTypeKind { get; }

        /// <summary>
        /// Gets the core property information about this template.
        /// </summary>
        /// <value>The property.</value>
        private PropertyInfo Property { get; }

        /// <inheritdoc cref="IGraphFieldResolverMetaData.Method" />
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets the set of parameters defined by <see cref="Method"/>.
        /// </summary>
        /// <value>The parameters.</value>
        public IReadOnlyList<ParameterInfo> Parameters { get; }

        /// <inheritdoc />
        public override IReadOnlyList<IGraphArgumentTemplate> Arguments { get; } = new List<IGraphArgumentTemplate>();
    }
}