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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Web;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A default implementation of the logic for handling a subscription request over a websocket.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class DefaultGraphQLHttpSubscriptionProcessor<TSchema> : IGraphQLHttpSubscriptionProcessor<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Accepts the post request and attempts to convert the body to a query data item.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        public virtual Task Invoke(HttpContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}