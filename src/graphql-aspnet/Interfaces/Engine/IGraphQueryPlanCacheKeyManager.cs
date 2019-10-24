// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An object that generates guaranteed unique keys "per query, per schema".
    /// </summary>
    public interface IGraphQueryPlanCacheKeyManager
    {
        /// <summary>
        /// Creates a garunteed unique key for the query text in context of hte schema.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the query is targeting.</typeparam>
        /// <param name="queryText">The query text.</param>
        /// <returns>System.String.</returns>
        string CreateKey<TSchema>(string queryText)
            where TSchema : class, ISchema;
    }
}