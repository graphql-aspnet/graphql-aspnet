// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Web;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// This asp.net handler invokes the graphql runtime in the context of a subscription
    /// and routes changes appropriately to any subscribed clients.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this handler is built for.</typeparam>
    public class GraphQLSubscriptionHandler<TSchema>
        where TSchema : class, ISchema
    {
      /// <summary>
        /// Invokes the handler and processes the request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        private Task Invoke(HttpContext context)
        {
            var processor = context.RequestServices.GetService(typeof(IGraphQLHttpSubscriptionProcessor<TSchema>))
                as IGraphQLHttpSubscriptionProcessor<TSchema>;

            if (processor == null)
            {
                throw new InvalidOperationException(
                    $"No {nameof(IGraphQLHttpSubscriptionProcessor)} of type " +
                    $"{typeof(IGraphQLHttpSubscriptionProcessor<TSchema>).FriendlyName()} " +
                    "is registered with the DI container. The GraphQL runtime cannot invoke " +
                    "subscriptions for the target schema.");
            }

            return processor.Invoke(context);
        }

        /// <summary>
        /// Creates the delegate invoker to process an HTTP Request.
        /// </summary>
        /// <param name="appBuilder">The application builder.</param>
        public void CreateInvoker(IApplicationBuilder appBuilder)
        {
            appBuilder.Run(this.Invoke);
        }
    }
}