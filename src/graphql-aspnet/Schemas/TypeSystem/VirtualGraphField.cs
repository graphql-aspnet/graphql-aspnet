﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
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
        /// <param name="parent">The parent graph type that owns this field.</param>
        /// <param name="fieldName">Name of the field in the object graph.</param>
        /// <param name="route">The path segment that represents this virtual field.</param>
        /// <param name="parentTypeName">The type name to use for the virtual type that owns this field.</param>
        public VirtualGraphField(
            IGraphType parent,
            string fieldName,
            SchemaItemPath route,
            string parentTypeName)
        {
            Validation.ThrowIfNull(route, nameof(route));
            parentTypeName = Validation.ThrowIfNullWhiteSpaceOrReturn(parentTypeName, nameof(parentTypeName));

            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));

            this.AssociatedGraphType = new VirtualObjectGraphType(parentTypeName);
            this.TypeExpression = new GraphTypeExpression(parentTypeName);
            this.Arguments = new GraphFieldArgumentCollection(this);
            this.Resolver = new GraphControllerRouteFieldResolver(new VirtualResolvedObject(this.TypeExpression.TypeName));

            // fields made from controller route parameters have no policies directly unto themselves
            // any controller class level policies are individually added to fields they declare
            this.SecurityGroups = new AppliedSecurityPolicyGroups();
            this.Complexity = 1;
            this.Mode = FieldResolutionMode.PerSourceItem;

            this.AppliedDirectives = new AppliedDirectiveCollection(this);
            this.Publish = true;
            this.IsDeprecated = false;
            this.DeprecationReason = null;
        }

        /// <inheritdoc />
        public void UpdateResolver(IGraphFieldResolver newResolver, FieldResolutionMode? mode = null)
        {
            throw new NotSupportedException($"The resolver for '{typeof(VirtualGraphField).FriendlyName()}' cannot be altered.");
        }

        /// <inheritdoc />
        public void AssignParent(IGraphType parent)
        {
            this.Parent = this.Parent;
        }

        /// <inheritdoc />
        public IGraphField Clone(IGraphType parent)
        {
            throw new NotImplementedException("Virtual Fields cannot be cloned.");
        }

        /// <inheritdoc />
        public virtual bool CanResolveForGraphType(IGraphType graphType)
        {
            // if the provided graphtype owns this virtual field
            // then yes it can resolve for it
            if (graphType is IGraphFieldContainer fieldContainer)
            {
                if (fieldContainer.Fields.Contains(this))
                    return true;
            }

            return false;
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
        public string Name { get; set; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; set; }

        /// <inheritdoc />
        public IGraphArgumentCollection Arguments { get; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public bool Publish { get; set; }

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
        public bool IsDeprecated { get; set; }

        /// <inheritdoc />
        public string DeprecationReason { get; set; }

        /// <inheritdoc />
        public IAppliedSecurityPolicyGroups SecurityGroups { get; }

        /// <inheritdoc />
        public float? Complexity { get; set; }

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

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }
    }
}