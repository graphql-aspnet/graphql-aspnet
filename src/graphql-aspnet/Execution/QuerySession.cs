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
    using GraphQL.AspNet.Interfaces.Execution;

    /// <inheritdoc cref="IQuerySession" />
    public sealed class QuerySession : IQuerySession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuerySession" /> class.
        /// </summary>
        /// <param name="items">The collection of data items to track in this session.</param>
        public QuerySession(MetaDataCollection items = null)
        {
            this.Items = items ?? new MetaDataCollection();
        }

        /// <inheritdoc />
        public MetaDataCollection Items { get; }
    }
}