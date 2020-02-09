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
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A default implementation of the logic for handling a subscription request over a websocket.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class DefaultGraphQLHttpSubscriptionMiddleware<TSchema>
        where TSchema : class, ISchema
    {
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
            this.Next = next;
            this.RoutePath = Validation.ThrowIfNullWhiteSpaceOrReturn(routePath, nameof(routePath));
            this.Options = Validation.ThrowIfNullOrReturn(options, nameof(options));
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
                this.RoutePath,
                CultureInfo.InvariantCulture,
                CompareOptions.OrdinalIgnoreCase) != 0)
            {
                await this.Next(context);
                return;
            }

            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                var subscriptionFactory = context.RequestServices.GetRequiredService(typeof(ISubscriptionClientFactory<TSchema>))
                    as ISubscriptionClientFactory<TSchema>;

                var socketProxy = new WebSocketClientConnection(webSocket);

                var subscriptionClient = subscriptionFactory.CreateClientProxy(context, socketProxy, this.Options);

                // hold the client connection until its released
                await subscriptionClient.StartConnection();
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(
                    $"The route '{this.RoutePath}', is configured to only accept subscription operation requests " +
                    $"for target schema '{typeof(TSchema).FriendlyName()}'. These requests" +
                    "must be sent via an appropriate websocket protocol.");
            }
        }

        /// <summary>
        /// Gets the delegate representing the next middleware component to invoke in the pipeline.
        /// </summary>
        /// <value>The next.</value>
        protected RequestDelegate Next { get; }

        /// <summary>
        /// Gets the subscription options configured for the target schema.
        /// </summary>
        /// <value>The options.</value>
        protected SchemaSubscriptionOptions<TSchema> Options { get; }

        /// <summary>
        /// Gets the fully qualified internal path representing the route to the subscription server. (e.g. '/graphql/subscriptions' ).
        /// </summary>
        /// <value>The route path.</value>
        protected string RoutePath { get; }
    }
}