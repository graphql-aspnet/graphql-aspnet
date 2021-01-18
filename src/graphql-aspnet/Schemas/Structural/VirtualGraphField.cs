// *************************************************************
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
        public VirtualGraphField(string fieldName, GraphFieldPath path, string typeName)
        {
            Validation.ThrowIfNull(path, nameof(path));
            Validation.ThrowIfNullWhiteSpace(typeName, nameof(typeName));
            this.Name = fieldName;
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

        /// <summary>
        /// Gets the tracked copy of the graph type that represents this virtual field.
        /// </summary>
        /// <value>The type of the associated graph.</value>
        public IObjectGraphType AssociatedGraphType { get; }

        /// <summary>
        /// Gets an object that will perform some operation against an execution
        /// context to fulfill the requirements of this resolvable entity.
        /// </summary>
        /// <value>The resolver assigned to this instance.</value>
        public IGraphFieldResolver Resolver { get; }

        /// <summary>
        /// Gets the resolution mode of this field which instructs the query plan executor on
        /// how to process this field against its source data.
        /// </summary>
        /// <value>The mode.</value>
        public FieldResolutionMode Mode { get; }

        /// <summary>
        /// Gets the name of this field as defined by the type declaration.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the route.
        /// </summary>
        /// <value>The route.</value>
        public GraphFieldPath Route { get; }

        /// <summary>
        /// Updates the field resolver used by this graph field.
        /// </summary>
        /// <param name="newResolver">The new resolver this field should use.</param>
        /// <param name="mode">The new resolution mode used by the runtime to invoke the resolver.</param>
        public void UpdateResolver(IGraphFieldResolver newResolver, FieldResolutionMode mode)
        {
            throw new InvalidOperationException($"The resolver for '{typeof(VirtualGraphField).FriendlyName()}' cannot be altered.");
        }

        /// <summary>
        /// Gets the type expression that represents the data returned from this field (i.e. the '[SomeType!]'
        /// declaration used in schema definition language.)
        /// </summary>
        /// <value>The type expression.</value>
        public GraphTypeExpression TypeExpression { get; }

        /// <summary>
        /// Gets the arguments this field can accept, if any.
        /// </summary>
        /// <value>The arguments.</value>
        public IGraphFieldArgumentCollection Arguments { get; }

        /// <summary>
        /// Gets or sets the description of this type field.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGraphField" /> is published
        /// in the schema delivered to introspection requests.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
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

        /// <summary>
        /// Gets a value indicating whether this instance is a leaf field; one capable of generating
        /// a real data item vs. generating data to be used in down stream projections.
        /// </summary>
        /// <value><c>true</c> if this instance is a leaf field; otherwise, <c>false</c>.</value>
        public bool IsLeaf => false;

        /// <summary>
        /// Gets a value indicating whether this item  is depreciated. The <see cref="DeprecationReason" /> will be displayed
        /// on any itnrospection requests.
        /// </summary>
        /// <value><c>true</c> if this instance is depreciated; otherwise, <c>false</c>.</value>
        public bool IsDeprecated => false;

        /// <summary>
        /// Gets the provided reason for this item being depreciated.
        /// </summary>
        /// <value>The depreciation reason.</value>
        public string DeprecationReason => null;

        /// <summary>
        /// Gets the security policies found via defined attributes on the item that need to be enforced.
        /// </summary>
        /// <value>The security policies.</value>
        public IEnumerable<FieldSecurityGroup> SecurityGroups { get; }

        /// <summary>
        /// Gets  an estimated weight value of this field in terms of the overall impact it has on the execution of a query.
        /// See the documentation for an understanding of how query complexity is calculated.
        /// </summary>
        /// <value>The estimated complexity value for this field.</value>
        public float? Complexity { get; }

        /// <summary>
        /// Gets the source type this field was created from.
        /// </summary>
        /// <value>The field souce.</value>
        public GraphFieldTemplateSource FieldSource => GraphFieldTemplateSource.None;

        /// <summary>
        /// Gets a collection of concrete types (and their associated type kinds) that this graph type is dependent on if/when a resolution against a field
        /// of this type occurs.
        /// </summary>
        /// <value>The dependent types.</value>
        public IEnumerable<DependentType> DependentTypes => REQUIRED_TYPES;

        /// <summary>
        /// Gets A collection of pre-created, abstract graph (Unions and Interfaces) types that this graph type is dependent on if/when a resolution
        /// against this type occurs.
        /// </summary>
        /// <value>The dependent graph types.</value>
        public IEnumerable<IGraphType> AbstractGraphTypes => Enumerable.Empty<IGraphType>();

        /// <summary>
        /// Gets a value indicating whether this instance is virtual and added by the runtime to facilitate
        /// a user defined graph structure. When false, this field points to developer created code.
        /// </summary>
        /// <value><c>true</c> if this instance is virtual; otherwise, <c>false</c>.</value>
        public bool IsVirtual => true;
    }
}