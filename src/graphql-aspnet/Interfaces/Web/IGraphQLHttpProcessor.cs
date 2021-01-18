// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Web
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

        /// <summary>
    /// A processor that is created from a DI container by a runtime handler to handle the individual
    /// request.
    /// </summary>
    public interface IGraphQLHttpProcessor
    {
        /// <summary>
        /// Accepts the post request and attempts to convert the body to a query data item.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        Task Invoke(HttpContext context);
    }
}