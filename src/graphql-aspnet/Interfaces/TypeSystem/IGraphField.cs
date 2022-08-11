// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using System;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// Describes a single field in the type system. This describes how a given field is to be represented with its
    /// accepted arguments, any nullable or list modifiers and publication and depreciation information.
    /// </summary>
    public interface IGraphField : IDeprecatable, IGraphArgumentContainer, ISecureSchemaItem, IGraphFieldBase
    {
        /// <summary>
        /// Updates the field resolver used by this graph field.
        /// </summary>
        /// <param name="newResolver">The new resolver this field should use.</param>
        /// <param name="mode">The new resolution mode used by the runtime to invoke the resolver.
        /// When null, the current resolution mode of this field is retained.</param>
        void UpdateResolver(IGraphFieldResolver newResolver, FieldResolutionMode? mode = null);

        /// <summary>
        /// Determines whether this field is capable of resolving itself
        /// for the given graph type.
        /// </summary>
        /// <param name="graphType">The graph type to test.</param>
        /// <returns><c>true</c> if this field can be returned by specified graph type; otherwise, <c>false</c>.</returns>
        bool CanResolveForGraphType(IGraphType graphType);

        /// <summary>
        /// Clones this instance to a new field.
        /// </summary>
        /// <param name="parent">The new parent item that will own this new field.</param>
        /// <returns>IGraphField.</returns>
        IGraphField Clone(IGraphType parent);

        /// <summary>
        /// Gets a value indicating whether this instance is a leaf field; one capable of generating
        /// a real data item vs. generating data to be used in down stream projections.
        /// </summary>
        /// <value><c>true</c> if this instance is a leaf field; otherwise, <c>false</c>.</value>
        bool IsLeaf { get; }

        /// <summary>
        /// <para>Gets an object that will perform some operation against an execution
        /// context to fulfill the requirements of this resolvable entity.
        /// </para>
        /// <remarks>
        /// Call <see cref="UpdateResolver"/> to change.
        /// </remarks>
        /// </summary>
        /// <value>The resolver assigned to this instance.</value>
        IGraphFieldResolver Resolver { get; }

        /// <summary>
        /// Gets the resolution mode of this field which instructs the query plan executor on
        /// how to process this field against its source data.
        /// </summary>
        /// <value>The mode.</value>
        FieldResolutionMode Mode { get; }

        /// <summary>
        /// Gets or sets an estimated weight value of this field in terms of the overall impact it has on the execution of a query.
        /// See the documentation for an understanding of how query complexity is calculated.
        /// </summary>
        /// <value>The estimated complexity value for this field.</value>
        float? Complexity { get; set; }

        /// <summary>
        /// Gets the source type this field was created from.
        /// </summary>
        /// <value>The field souce.</value>
        GraphFieldSource FieldSource { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is virtual and added by the runtime to facilitate
        /// a user defined graph structure. When false, this field points to developer created code.
        /// </summary>
        /// <value><c>true</c> if this instance is virtual; otherwise, <c>false</c>.</value>
        bool IsVirtual { get; }
    }
}