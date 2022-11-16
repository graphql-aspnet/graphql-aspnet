﻿// *************************************************************
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
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
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

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            this.ExpectedReturnType = GraphValidation.EliminateWrappersFromCoreType(
                this.DeclaredReturnType,
                false,
                true,
                false);
        }

        /// <inheritdoc />
        public override IGraphFieldResolver CreateResolver()
        {
            return new GraphObjectPropertyResolver(this);
        }

        /// <inheritdoc />
        public override Type DeclaredReturnType => this.Property.PropertyType;

        /// <inheritdoc />
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

        /// <inheritdoc />
        public MethodInfo Method { get; }

        /// <inheritdoc />
        public IReadOnlyList<ParameterInfo> Parameters { get; }

        /// <inheritdoc />
        public override IReadOnlyList<IGraphArgumentTemplate> Arguments { get; } = new List<IGraphArgumentTemplate>();

        /// <inheritdoc />
        public override string InternalFullName => $"{this.Parent.InternalFullName}.{this.Property.Name}";

        /// <inheritdoc />
        public override string InternalName => this.Property.Name;
    }
}