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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A representation of a object field as it would be defined in the graph type system.
    /// </summary>
    /// <seealso cref="IGraphField" />
    [DebuggerDisplay("Field: {Name}")]
    public class MethodGraphField : IGraphField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodGraphField" /> class.
        /// </summary>
        /// <param name="fieldName">Name of the field in the graph.</param>
        /// <param name="typeExpression">The meta data describing the type of data this field returns.</param>
        /// <param name="route">The formal route to this field in the object graph.</param>
        /// <param name="mode">The mode in which the runtime will process this field.</param>
        /// <param name="resolver">The resolver to be invoked to produce data when this field is called.</param>
        /// <param name="securityPolicies">The security policies that apply to this field.</param>
        public MethodGraphField(
            string fieldName,
            GraphTypeExpression typeExpression,
            GraphFieldPath route,
            FieldResolutionMode mode = FieldResolutionMode.PerSourceItem,
            IGraphFieldResolver resolver = null,
            IEnumerable<FieldSecurityGroup> securityPolicies = null)
        {
            this.Name = fieldName;
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.Arguments = new GraphFieldArgumentCollection();
            this.SecurityGroups = securityPolicies ?? Enumerable.Empty<FieldSecurityGroup>();
            this.UpdateResolver(resolver, mode);
        }

        /// <summary>
        /// Gets the name of this field as defined by the type declaration.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Updates the field resolver used by this graph field.
        /// </summary>
        /// <param name="newResolver">The new resolver this field should use.</param>
        /// <param name="mode">The new resolution mode used by the runtime to invoke the resolver.</param>
        public void UpdateResolver(IGraphFieldResolver newResolver, FieldResolutionMode mode)
        {
            this.Resolver = newResolver;
            this.Mode = mode;

            var unrwrappedType = GraphValidation.EliminateWrappersFromCoreType(this.Resolver?.ObjectType);
            this.IsLeaf = this.Resolver?.ObjectType != null && GraphQLProviders.ScalarProvider.IsLeaf(unrwrappedType);
        }

        /// <summary>
        /// Gets the type expression that represents the data returned from this field (i.e. the '[SomeType!]'
        /// declaration used in schema definition language.)
        /// </summary>
        /// <value>The type expression.</value>
        public GraphTypeExpression TypeExpression { get; }

        /// <summary>
        /// Gets the security policies that must be passed in order to allow access to this field.
        /// </summary>
        /// <value>The security policies.</value>
        public IEnumerable<FieldSecurityGroup> SecurityGroups { get; }

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
        public virtual bool Publish => true;

        /// <summary>
        /// Gets the route assigned to this field in the object graph.
        /// </summary>
        /// <value>The route.</value>
        public GraphFieldPath Route { get; }

        /// <summary>
        /// Gets an object that will perform some operation against an execution
        /// context to fulfill the requirements of this resolvable entity.
        /// </summary>
        /// <value>The resolver assigned to this instance.</value>
        public IGraphFieldResolver Resolver { get; private set; }

        /// <summary>
        /// Gets the resolution mode of this field which instructs the query plan executor on
        /// how to process this field against its source data.
        /// </summary>
        /// <value>The mode.</value>
        public FieldResolutionMode Mode { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is a leaf field; one capable of generating
        /// a real data item vs. generating data to be used in down stream projections.
        /// </summary>
        /// <value><c>true</c> if this instance is a leaf field; otherwise, <c>false</c>.</value>
        public bool IsLeaf { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this item  is depreciated. The <see cref="DeprecationReason" /> will be displayed
        /// on any itnrospection requests.
        /// </summary>
        /// <value><c>true</c> if this instance is depreciated; otherwise, <c>false</c>.</value>
        public bool IsDeprecated { get; internal set; }

        /// <summary>
        /// Gets the provided reason for this item being depreciated.
        /// </summary>
        /// <value>The depreciation reason.</value>
        public string DeprecationReason { get; internal set; }

        /// <summary>
        /// Gets an estimated weight value of this field in terms of the overall impact it has on the execution of a query.
        /// See the documentation for an understanding of how query complexity is calculated.
        /// </summary>
        /// <value>The estimated complexity value for this field.</value>
        public float? Complexity { get; internal set; }

        /// <summary>
        /// Gets the source type this field was created from.
        /// </summary>
        /// <value>The field souce.</value>
        public GraphFieldTemplateSource FieldSource { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this instance is virtual and added by the runtime to facilitate
        /// a user defined graph structure. When false, this field points to developer created code.
        /// </summary>
        /// <value><c>true</c> if this instance is virtual; otherwise, <c>false</c>.</value>
        public bool IsVirtual => false;
    }
}