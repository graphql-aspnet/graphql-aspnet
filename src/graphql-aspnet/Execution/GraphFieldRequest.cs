// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// The default implementation of <see cref="IGraphFieldRequest"/>. Provides a container
    /// for various data fields needed to resolve a field.
    /// </summary>
    [DebuggerDisplay("{InvocationContext.Field.Name}, (Leaf = {InvocationContext.Field.IsLeaf})")]
    [DebuggerStepThrough]
    public class GraphFieldRequest : IGraphFieldRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldRequest" /> class.
        /// </summary>
        /// <param name="invocationContext">The invocation context that defines how hte field
        /// should be processed according to the query plan.</param>
        /// <param name="dataSource">The data source containing the the source input data to the field as well as
        /// the graph items referenced by said input data.</param>
        /// <param name="origin">The location in the source query where this field request was generated.</param>
        /// <param name="items">A collection of meta data items to carry with this request.</param>
        public GraphFieldRequest(
            IGraphFieldInvocationContext invocationContext,
            GraphFieldDataSource dataSource,
            SourceOrigin origin,
            MetaDataCollection items = null)
        {
            this.Id = Guid.NewGuid().ToString("N");
            this.InvocationContext = Validation.ThrowIfNullOrReturn(invocationContext, nameof(invocationContext));
            this.Origin = Validation.ThrowIfNullOrReturn(origin, nameof(origin));
            this.Items = items ?? new MetaDataCollection();
            this.DataSource = dataSource;
        }

        /// <summary>
        /// Gets the globally unique Id assigned to this individual field request.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; }

        /// <summary>
        /// Gets any additional metadata or items assigned to this request.
        /// </summary>
        /// <value>The metadata.</value>
        public MetaDataCollection Items { get; }

        /// <summary>
        /// Gets the origin point in the source text where this request was generated.
        /// </summary>
        /// <value>The origin.</value>
        public SourceOrigin Origin { get; }

        /// <summary>
        /// Gets the source data item feeding this request.
        /// </summary>
        /// <value>The source data.</value>
        public GraphFieldDataSource DataSource { get; }

        /// <summary>
        /// Gets the field referenced by the invocation context of this request.
        /// </summary>
        /// <value>The field.</value>
        public IGraphField Field => this.InvocationContext.Field;

        /// <summary>
        /// Gets the data related to what field needs to be processed and how it should be
        /// processed according to its definition in the query document from which it was parsed.
        /// </summary>
        /// <value>The invocation data.</value>
        public IGraphFieldInvocationContext InvocationContext { get; }
    }
}