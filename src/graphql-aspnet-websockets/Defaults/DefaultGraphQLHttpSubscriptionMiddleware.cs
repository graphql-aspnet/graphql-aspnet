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
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Web;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLHttpSubscriptionMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="next">The delegate pointing to the next middleware component
        /// in the pipeline.</param>
        /// <param name="routePath">The route path where subscriptions should be pointed.</param>
        public DefaultGraphQLHttpSubscriptionMiddleware(RequestDelegate next, string routePath)
        {
            _next = next;
            _routePath = routePath;
        }

        /// <summary>
        /// The middleware invokation function that can process the current http context
        /// if it should be handled as a subscription request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        public virtual async Task InvokeAsync(HttpContext context)
        {
            // immediate bypass if not aimed at this schema subscription route
            if (context.Request.Path != _routePath)
            {
                await _next(context);
                return;
            }

            if (context.WebSockets.IsWebSocketRequest)
            {
                // do web socket stuff
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await this.Echo(context, webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("This route is configured to only accept web socket requests.");
            }
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}