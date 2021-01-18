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
    /// A processor that can respond to an incoming request for subscription related schema data.
    /// </summary>
    public interface ISubscriptionSchemaHttpProcessor
    {
        /// <summary>
        /// Invokes the processor to handle the request on the given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        Task Invoke(HttpContext context);
    }
}