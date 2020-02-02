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
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Messaging;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A default implementation of the logic for handling a subscription request over a websocket.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class DefaultGraphQLHttpSubscriptionMiddleware<TSchema>
        where TSchema : class, ISchema
    {
        private readonly RequestDelegate _next;
        private readonly string _routePath;
        private readonly SchemaSubscriptionOptions<TSchema> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLHttpSubscriptionMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="next">The delegate pointing to the next middleware component
        /// in the pipeline.</param>
        /// <param name="options">The options.</param>
        /// <param name="routePath">The route path where subscriptions should be pointed.</param>
        public DefaultGraphQLHttpSubscriptionMiddleware(
            RequestDelegate next,
            SchemaSubscriptionOptions<TSchema> options,
            string routePath)
        {
            _next = next;
            _routePath = Validation.ThrowIfNullWhiteSpaceOrReturn(routePath, nameof(routePath));
            _options = Validation.ThrowIfNullOrReturn(options, nameof(options));
        }

        /// <summary>
        /// The invocation method that can process the current http context
        /// if it should be handled as a subscription request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        public virtual async Task InvokeAsync(HttpContext context)
        {
            // immediate bypass if not aimed at this schema subscription route
            if (string.Compare(
                context.Request.Path,
                _routePath,
                CultureInfo.InvariantCulture,
                CompareOptions.OrdinalIgnoreCase) != 0)
            {
                await _next(context);
                return;
            }

            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var subscription = new ApolloSubscriptionRegistration<TSchema>(context, webSocket, _options);
                await subscription.MonitorSubscription();
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(
                    $"This route, '{_routePath}', is configured to only accept subscription operation requests " +
                    $"for target schema '{typeof(TSchema).FriendlyName()}'. These requests" +
                    "must be sent via an appropriate websocket protocol.");
            }
        }
    }
}