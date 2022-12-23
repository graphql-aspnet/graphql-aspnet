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
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// An object that is created from a DI container by a runtime handler to handle the graphql
    /// individual received via an API end point.
    /// </summary>
    public interface IGraphQLHttpProcessor
    {
        /// <summary>
        /// Accepts the post request and attempts to convert the body to a query data item.
        /// </summary>
        /// <param name="context">The context to process.</param>
        /// <returns>Task.</returns>
        Task InvokeAsync(HttpContext context);
    }
}