// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// The default implementation of the cache key manager. This key manager is used unless overriden by
    /// a user's individual configuration.
    /// </summary>
    public class DefaultQueryPlanCacheKeyManager : IGraphQueryPlanCacheKeyManager
    {
        private readonly IGraphQLDocumentParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultQueryPlanCacheKeyManager"/> class.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public DefaultQueryPlanCacheKeyManager(IGraphQLDocumentParser parser)
        {
            _parser = parser;
        }

        /// <summary>
        /// Creates a garunteed unique key for the query text in context of hte schema.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the query is targeting.</typeparam>
        /// <param name="queryText">The query text.</param>
        /// <returns>System.String.</returns>
        public string CreateKey<TSchema>(string queryText)
             where TSchema : class, ISchema
        {
            return $"{typeof(TSchema).Name}_{_parser.StripInsignificantWhiteSpace(queryText)}";
        }
    }
}