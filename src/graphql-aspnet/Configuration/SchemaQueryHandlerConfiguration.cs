﻿// *************************************************************
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
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Web;

    /// <summary>
    /// Configuration options related to how the built in query handler is setup and
    /// processes GraphQL requests.
    /// </summary>
    [DebuggerDisplay("Route = '{Route}'")]
    public class SchemaQueryHandlerConfiguration
    {
        private Type _httpProcessorType;

        /// <summary>
        /// <para>
        /// Gets or sets a value indicating whether the default Http Processor
        /// should be registered to the application. When disabled, the application will not register
        /// its query handler as a public end point; the application will need to handle
        /// HTTP request routing manually.
        /// </para>
        /// <para>
        /// Default: false  (i.e. "DO INCLUDE the default route").
        /// </para>
        /// </summary>
        /// <value><c>true</c> if the route should be disabled; otherwise, <c>false</c>.</value>
        public bool DisableDefaultRoute { get; set; } = false;

        /// <summary>
        /// <para>
        /// Gets or sets the url path on which the schema will listen for requests.
        /// The route is automatically registered for POST and GET request.
        /// </para>
        /// <para>
        /// NOTE: If this application registers more than one <see cref="ISchema"/> this value must be unique
        /// for each schema registered; otherwise, an exception will be thrown.
        /// </para>
        /// <para>
        /// Default: "/graphql".
        /// </para>
        /// </summary>
        /// <value>The path to listen to for graphql requests.</value>
        public string Route { get; set; } = Constants.Routing.DEFAULT_HTTP_ROUTE;

        /// <summary>
        /// <para>
        /// Gets or sets an optional .NET type to use as the processor for HTTP requests. When set,
        /// this type must inherit from <see cref="IGraphQLHttpProcessor{TSchema}" />.
        /// </para>
        /// <para>
        /// It can be advantageous to override the built in <see cref="DefaultGraphQLHttpProcessor{TSchema}" /> and register your custom
        /// type here rather than overriding the entire graphql http middleware component. See the documentation for further details.
        /// </para>
        /// <para>
        /// (Default: null).
        /// </para>
        /// </summary>
        /// <value>The type of the processor to use when processing HTTP Requests received by the application.</value>
        public Type HttpProcessorType
        {
            get
            {
                return _httpProcessorType;
            }

            set
            {
                Validation.ThrowIfNotCastable<IGraphQLHttpProcessor>(value, nameof(HttpProcessorType));
                _httpProcessorType = value;
            }
        }

        /// <summary>
        /// <para>
        /// Gets or sets a value indicating whether the the default http processor
        /// will only accept authenticated requests.  When <c>true</c>, the graphql runtime will not be invoked
        /// if the requestor has not been authenticated by ASP.NET.
        /// </para>
        /// <para>
        /// (Default: false).
        /// </para>
        /// </summary>
        /// <remarks>
        /// This setting has no effect if you define a custom <see cref="HttpProcessorType"/>.
        /// </remarks>
        /// <value><c>true</c> if only authorized requests should be handled by graphql; otherwise, <c>false</c>.</value>
        public bool AuthenticatedRequestsOnly { get; set; }
    }
}