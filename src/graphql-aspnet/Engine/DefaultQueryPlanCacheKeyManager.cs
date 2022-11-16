// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;

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

        /// <inheritdoc />
        public string CreateKey<TSchema>(string queryText, string operationName)
             where TSchema : class, ISchema
        {
            operationName = operationName?.Trim() ?? string.Empty;

            // tilde (~) isn't allowed in an operaiton name
            // making hte entry unique for a request that doesnt specify an operation name
            if (!string.IsNullOrEmpty(operationName))
                operationName = $"Operation-{operationName}";
            else
                operationName = "Operation~NONE";

            return $"{typeof(TSchema).Name}_{operationName}_{_parser.StripInsignificantWhiteSpace(queryText)}";
        }
    }
}