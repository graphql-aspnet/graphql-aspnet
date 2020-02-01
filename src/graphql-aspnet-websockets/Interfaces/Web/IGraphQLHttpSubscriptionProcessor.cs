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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A processor acting on a request to perform a subscription operation.
    /// </summary>
    public interface IGraphQLHttpSubscriptionProcessor
    {
        /// <summary>
        /// Accepts the post request and attempts to convert the body to a query data item.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        Task Invoke(HttpContext context);
    }
}