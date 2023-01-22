// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;

    /// <summary>
    /// A payload containing all the information to execute one or more graphql
    /// through the <see cref="MultipartRequestGraphQLHttpProcessor{TSchema}"/>.
    /// </summary>
    public class MultiPartRequestGraphQLPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPartRequestGraphQLPayload"/> class.
        /// </summary>
        public MultiPartRequestGraphQLPayload()
        {
            this.QueriesToExecute = new List<GraphQueryData>();
            this.IsBatch = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPartRequestGraphQLPayload"/> class.
        /// </summary>
        /// <param name="queryData">The single, non-batch query data object to execute.</param>
        public MultiPartRequestGraphQLPayload(GraphQueryData queryData)
        {
            Validation.ThrowIfNull(queryData, nameof(queryData));
            var list = new List<GraphQueryData>(1);
            list.Add(queryData);

            this.QueriesToExecute = list;
            this.IsBatch = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPartRequestGraphQLPayload"/> class.
        /// </summary>
        /// <param name="queryData">A set of query data items to execute as a batch.</param>
        public MultiPartRequestGraphQLPayload(List<GraphQueryData> queryData)
        {
            Validation.ThrowIfNull(queryData, nameof(queryData));

            var list = new List<GraphQueryData>(queryData);

            this.QueriesToExecute = list;
            this.IsBatch = queryData.Count > 0;
        }

        /// <summary>
        /// Gets a collection of queries to execute against the graphql runtime.
        /// </summary>
        /// <value>The queries to execute.</value>
        public virtual IReadOnlyList<GraphQueryData> QueriesToExecute { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is request is executed in batch mode.
        /// Regardless of the number of <see cref="QueriesToExecute"/> a payload executed in batch mode
        /// will result in an array of results being returned to the caller, even if it is an array of 0 items.
        /// </summary>
        /// <value><c>true</c> if this instance is batch; otherwise, <c>false</c>.</value>
        public virtual bool IsBatch { get; }
    }
}