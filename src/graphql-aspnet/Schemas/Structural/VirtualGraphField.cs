﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// An representation of a field on an object graph that maps to no concrete structure in the application. Typically used
    /// for nesting controller actions on lengthy route paths.
    /// </summary>
    [DebuggerDisplay("Virtual Field: {Route.Path}")]
    public class VirtualGraphField : IGraphField, IGraphItemDependencies
    {
        private static readonly IList<DependentType> REQUIRED_TYPES;

        /// <summary>
        /// Initializes static members of the <see cref="VirtualGraphField"/> class.
        /// </summary>
        static VirtualGraphField()
        {
            REQUIRED_TYPES = new List<DependentType>();
            REQUIRED_TYPES.Add(new DependentType(typeof(VirtualResolvedObject), TypeKind.OBJECT));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGraphField" /> class.
        /// </summary>
        /// <param name="fieldName">Name of the field in the object graph.</param>
        /// <param name="path">The path segment this virtual field will represent.</param>
        /// <param name="typeName">The type name to use for the virtual type generated from this route field.</param>
        public VirtualGraphField(
            string fieldName,
            GraphFieldPath path,
            string typeName)
        {
            Validation.ThrowIfNull(path, nameof(path));
            Validation.ThrowIfNullWhiteSpace(typeName, nameof(typeName));

            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));
            this.Route = Validation.ThrowIfNullOrReturn(path, nameof(path));

            this.AssociatedGraphType = new VirtualObjectGraphType(typeName);
            this.TypeExpression = new GraphTypeExpression(typeName);
            this.Arguments = new GraphFieldArgumentCollection();
            this.Resolver = new GraphRouteFieldResolver(new VirtualResolvedObject(this.TypeExpression.TypeName));

            // fields made from controller route parameters have no policies directly unto themselves
            // any controller class level policies are individually added to fields they declare
            this.SecurityGroups = Enumerable.Empty<FieldSecurityGroup>();
            this.Complexity = 1;
            this.Mode = FieldResolutionMode.PerSourceItem;
        }

        /// <inheritdoc />
        public void UpdateResolver(IGraphFieldResolver newResolver, FieldResolutionMode mode)
        {
            throw new InvalidOperationException($"The resolver for '{typeof(VirtualGraphField).FriendlyName()}' cannot be altered.");
        }

        /// <inheritdoc />
        public void AssignParent(IGraphType parent)
        {
            this.Parent = Parent;
        }

        /// <summary>
        /// Gets the tracked copy of the graph type that represents this virtual field.
        /// </summary>
        /// <value>The type of the associated graph.</value>
        public IObjectGraphType AssociatedGraphType { get; }

        /// <inheritdoc />
        public IGraphFieldResolver Resolver { get; }

        /// <inheritdoc />
        public FieldResolutionMode Mode { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public GraphFieldPath Route { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; }

        /// <inheritdoc />
        public IGraphFieldArgumentCollection Arguments { get; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public bool Publish => true;

        /// <summary>
        /// Gets or sets a value indicating whether this action method is depreciated. The <see cref="DepreciationReason" /> will be displayed
        /// on any itnrospection requests.
        /// </summary>
        /// <value><c>true</c> if this instance is depreciated; otherwise, <c>false</c>.</value>
        public bool IsDepreciated { get; set; }

        /// <summary>
        /// Gets or sets the provided reason for this action method being depreciated.
        /// </summary>
        /// <value>The depreciation reason.</value>
        public string DepreciationReason { get; set; }

        /// <inheritdoc />
        public bool IsLeaf => false;

        /// <inheritdoc />
        public bool IsDeprecated => false;

        /// <inheritdoc />
        public string DeprecationReason => null;

        /// <inheritdoc />
        public IEnumerable<FieldSecurityGroup> SecurityGroups { get; }

        /// <inheritdoc />
        public float? Complexity { get; }

        /// <inheritdoc />
        public GraphFieldSource FieldSource => GraphFieldSource.Virtual;

        /// <inheritdoc />
        public IEnumerable<DependentType> DependentTypes => REQUIRED_TYPES;

        /// <inheritdoc />
        public IEnumerable<IGraphType> AbstractGraphTypes => Enumerable.Empty<IGraphType>();

        /// <inheritdoc />
        public bool IsVirtual => true;

        /// <inheritdoc />
        public Type ObjectType => null;

        /// <inheritdoc />
        public Type DeclaredReturnType => null;

        /// <inheritdoc />
        public ISchemaItem Parent { get; private set; }
    }
}