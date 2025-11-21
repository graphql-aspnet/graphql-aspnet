// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    /// <summary>
    /// A test controller with deprecated arguments for testing introspection.
    /// </summary>
    public class DeprecatedArgumentsController : GraphController
    {
        /// <summary>
        /// A query method with a deprecated argument.
        /// </summary>
        /// <param name="query">The active search query.</param>
        /// <param name="legacySearch">A deprecated search parameter.</param>
        /// <param name="limit">The maximum number of results.</param>
        /// <returns>The search query string.</returns>
        [QueryRoot]
        public string Search(
            string query,
            [Deprecated("Use 'query' parameter instead")] string legacySearch = null,
            int limit = 10)
        {
            return query ?? legacySearch;
        }

        /// <summary>
        /// A query method with no deprecated arguments.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <returns>The id value.</returns>
        [QueryRoot]
        public int GetItem(int id)
        {
            return id;
        }
    }
}
