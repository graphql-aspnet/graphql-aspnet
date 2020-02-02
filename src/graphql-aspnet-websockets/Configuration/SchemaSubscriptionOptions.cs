// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Web;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Configuration options relating to subscriptions for a given schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class SchemaSubscriptionOptions<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaSubscriptionOptions{TSchema}"/> class.
        /// </summary>
        public SchemaSubscriptionOptions()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the default subscription processing handler
        /// should be registered to the application. If disabled, the application will not register
        /// its internal handler as a public end point for handling subscriptions. The application will need to handle
        /// websocket request routing manually (Default: false).
        /// </summary>
        /// <value><c>true</c> if the default subscription route should be disabled; otherwise, <c>false</c>.</value>
        public bool DisableDefaultRoute { get; set; } = false;

        /// <summary>
        /// Gets or sets the route path where any subscription clients should connect. use <see cref="SubscriptionConstants.Routing.SCHEMA_ROUTE_KEY"/>
        /// as part of your route to represent the primary configured route for this schema. (Default: "{schemaRoute}/subscriptions").
        /// </summary>
        /// <value>The route.</value>
        public string SubscriptionRoute { get; set; } = SubscriptionConstants.Routing.DEFAULT_SUBSCRIPTIONS_ROUTE;

        /// <summary>
        /// <para>Gets or sets an optional .NET type to use as the processor for Subscription requests. When set,
        /// this type should accept as constructors a <see cref="RequestDelegate"/> and a <see cref="string"/> representing
        /// the configured route for the subscription as constructor parameters.
        /// </para>
        /// <para>It can be advantagous to override the default  <see cref="DefaultGraphQLHttpSubscriptionMiddleware{TSchema}" /> and register your custom
        /// type here rather than overriding the entire subscription middleware component. See the documentation for further details.</para>
        /// </summary>
        /// <value>The type of the middleware component to use when processing WebSocket Requests received by the application for subscriptions.</value>
        public Type HttpProcessorType { get; set; } = null;
    }
}