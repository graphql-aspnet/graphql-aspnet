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
    /// A data item container, that may represent a collection of data items or just one singular item,
    /// that acts as a target to operate against for various invocations.
    /// </summary>
    [DebuggerDisplay("{Path} (Count: {Items.Count})")]
    public class GraphDataContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDataContainer" /> class.
        /// </summary>
        /// <param name="rawData">The singular raw data item which will be passed as the target data to the resolver of this request.</param>
        /// <param name="path">The path in the object graph pointed to by this data container.</param>
        /// <param name="dataItems">The set of data items making up the <paramref name="rawData"/> object. May be null
        /// if the <paramref name="rawData"/> is not an enumerable object.</param>
        public GraphDataContainer(
            object rawData,
            SourcePath path,
            params GraphDataItem[] dataItems)
        {
            this.Path = path;
            this.Value = rawData;
            this.Items = new List<GraphDataItem>(dataItems);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDataContainer" /> class.
        /// </summary>
        /// <param name="rawData">The singular raw data item which will be passed as the target data to the resolver of this request.</param>
        /// <param name="path">The path in the object graph pointed to by this data container.</param>
        /// <param name="dataItems">The set of data items making up the <paramref name="rawData"/> object. May be null
        /// if the <paramref name="rawData"/> is not an enumerable object.</param>
        public GraphDataContainer(
            object rawData,
            SourcePath path,
            IEnumerable<GraphDataItem> dataItems)
        {
            this.Path = path;
            this.Value = rawData;
            this.Items = dataItems == null
                ? new List<GraphDataItem>()
                : new List<GraphDataItem>(dataItems);
        }

        /// <summary>
        /// Gets the path pointed to by this container.
        /// </summary>
        /// <value>The path.</value>
        public SourcePath Path { get; }

        /// <summary>
        /// Gets the data value passed on this container.
        /// </summary>
        /// <value>The raw data item.</value>
        public object Value { get; }

        /// <summary>
        /// Gets the collection of data items being passed with this container.
        /// </summary>
        /// <value>The set of individual data items represented by the <see cref="Value"/>.</value>
        public List<GraphDataItem> Items { get; }
    }
}