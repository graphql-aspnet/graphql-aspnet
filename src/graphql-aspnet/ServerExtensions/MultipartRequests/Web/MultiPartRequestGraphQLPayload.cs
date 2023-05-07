// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Web
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine;

    /// <summary>
    /// A payload containing all the information to execute one or more graphql requests
    /// through the <see cref="MultipartRequestGraphQLHttpProcessor{TSchema}"/>.
    /// </summary>
    public class MultiPartRequestGraphQLPayload : IReadOnlyList<GraphQueryData>
    {
        private readonly List<GraphQueryData> _queries;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPartRequestGraphQLPayload"/> class.
        /// </summary>
        /// <param name="queryData">The single, non-batch query data object to execute.</param>
        public MultiPartRequestGraphQLPayload(GraphQueryData queryData)
        {
            Validation.ThrowIfNull(queryData, nameof(queryData));
            _queries = new List<GraphQueryData>(1);
            _queries.Add(queryData);
            this.IsBatch = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPartRequestGraphQLPayload"/> class.
        /// </summary>
        /// <param name="queryData">A set of query data items to execute as a batch.</param>
        public MultiPartRequestGraphQLPayload(IEnumerable<GraphQueryData> queryData)
        {
            queryData = queryData ?? Enumerable.Empty<GraphQueryData>();
            _queries = new List<GraphQueryData>(queryData);
            this.IsBatch = true;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is request is executed in batch mode.
        /// Regardless of the number of queries in this payload, when executed in batch mode
        /// it will result in an array of results being returned to the caller, even if it is an array of 0 items.
        /// </summary>
        /// <value><c>true</c> if this instance is batch; otherwise, <c>false</c>.</value>
        public virtual bool IsBatch { get; }

        /// <inheritdoc />
        public GraphQueryData this[int index] => _queries[index];

        /// <inheritdoc />
        public int Count => _queries.Count;

        /// <inheritdoc />
        public IEnumerator<GraphQueryData> GetEnumerator()
        {
            return _queries.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}