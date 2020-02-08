﻿// *************************************************************
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
    /// This handler is registered to the ASP.NET runtime to field requests for subscription schema
    /// information from an out-of-process subscription server.
    /// </summary>
    /// <typeparam name="TSchema">The schema type this handler works for.</typeparam>
    public class GraphQLSubscriptionSchemaRequestHandler<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Invokes the handler and processes the request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        private Task Invoke(HttpContext context)
        {
            var processor = context.RequestServices.GetService(typeof(ISubscriptionSchemaHttpProcessor<TSchema>))
                as ISubscriptionSchemaHttpProcessor<TSchema>;

            if (processor == null)
            {
                throw new InvalidOperationException(
                    $"No {nameof(ISubscriptionSchemaHttpProcessor)} of type " +
                    $"{typeof(ISubscriptionSchemaHttpProcessor<TSchema>).FriendlyName()} " +
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