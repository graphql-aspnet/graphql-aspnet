// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Web
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Web;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A publically available request delegate that acts as the primary entry point for receiving a graph query from an HTTP Request
    /// and processing it against the runtime.
    /// </summary>
    /// <typeparam name="TSchema">The schema type this handler works for.</typeparam>
    public class GraphQueryHandler<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Invokes the handler and processes the request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        private Task Invoke(HttpContext context)
        {
            var processor = context.RequestServices.GetService(typeof(IGraphQLHttpProcessor<TSchema>))
                as IGraphQLHttpProcessor<TSchema>;

            if (processor == null)
            {
                throw new InvalidOperationException(
                    $"No {nameof(IGraphQLHttpProcessor)} of type " +
                    $"{typeof(IGraphQLHttpProcessor<TSchema>).FriendlyName()} " +
                    "is registered with the DI container. The GraphQL runtime cannot invoke the schema.");
            }

            return processor.Invoke(context);
        }

        /// <summary>
        /// Creates the delegate invoker to process an HTTP Request.
        /// </summary>
        /// <param name="appBuilder">The application builder.</param>
        public void Execute(IApplicationBuilder appBuilder)
        {
            appBuilder.Run(this.Invoke);
        }
    }
}