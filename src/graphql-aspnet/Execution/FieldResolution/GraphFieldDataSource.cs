// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.FieldResolution
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// An item, that may contain a collection of data items or just one, that acts as an input source to a
    /// field execution pipeline.
    /// </summary>
    [DebuggerDisplay("{Path} (Count: {Items.Count})")]
    public class GraphFieldDataSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldDataSource" /> class.
        /// </summary>
        /// <param name="rawSource">The singular raw source item which will be passed as the source data to the resolver of this request.</param>
        /// <param name="path">The path in the object graph pointed to by this data source.</param>
        /// <param name="dataItems">The set of data items making up the raw source.</param>
        public GraphFieldDataSource(
            object rawSource,
            SourcePath path,
            params GraphDataItem[] dataItems)
        {
            this.Path = path;
            this.Value = rawSource;
            this.Items = new List<GraphDataItem>(dataItems);
        }

                /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldDataSource" /> class.
        /// </summary>
        /// <param name="rawSource">The singular raw source item which will be passed as the source data to the resolver of this request.</param>
        /// <param name="path">The path in the object graph pointed to by this data source.</param>
        /// <param name="dataItems">The set of data items making up the raw source.</param>
        public GraphFieldDataSource(
            object rawSource,
            SourcePath path,
            IEnumerable<GraphDataItem> dataItems)
        {
            this.Path = path;
            this.Value = rawSource;
            this.Items = dataItems == null ? new List<GraphDataItem>() : new List<GraphDataItem>(dataItems);
        }

        /// <summary>
        /// Gets the path pointed to by this data source.
        /// </summary>
        /// <value>The path.</value>
        public SourcePath Path { get; }

        /// <summary>
        /// Gets the raw source value passed on this data source.
        /// </summary>
        /// <value>The source data.</value>
        public object Value { get; }

        /// <summary>
        /// Gets the collection of data items being passed with this source container.
        /// </summary>
        /// <value>The items.</value>
        public List<GraphDataItem> Items { get; }
    }
}