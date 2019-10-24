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
    using System.Diagnostics;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Web;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Configuration options related to how the built in query handler is setup and
    /// processes GraphQL requests.
    /// </summary>
    [DebuggerDisplay("Route = '{Route}'")]
    public class SchemaQueryHandlerConfiguration
    {
        private Type _httpProcessorType;

        /// <summary>
        /// Occurs when a type reference is set to this configuration section that requires injection into the service collection.
        /// </summary>
        internal event EventHandler<TypeReferenceEventArgs> TypeReferenceAdded;

        /// <summary>
        /// Gets or sets a value indicating whether the default query processing controller
        /// should be registered to the application. If disabled, the application will not register
        /// its internal handler as a public end point; the application will need to handle
        /// HTTP request routing manually (Default: false).
        /// </summary>
        /// <value><c>true</c> if the route should be disabled; otherwise, <c>false</c>.</value>
        public bool DisableDefaultRoute { get; set; } = false;

        /// <summary>
        /// <para>
        /// Gets or sets the route to which the internal controller registered for the schema will listen.
        /// The route is automatically registered as a POST request. (Default: '/graphql').
        /// </para>
        /// <para>
        /// NOTE: If this application registers more than one <see cref="ISchema"/> this value must be unique
        /// for each schema registered; otherwise, an exception will be thrown.
        /// </para>
        /// </summary>
        /// <value>The path to listen for graphql requests.</value>
        public string Route { get; set; } = Constants.Routing.DEFAULT_HTTP_ROUTE;

        /// <summary>
        /// <para>Gets or sets an optional .NET type to use as the processor for HTTP requests. When set,
        /// this type must inherit from <see cref="IGraphQLHttpProcessor{TSchema}" />.(Default: null).
        /// </para>
        /// <para>It can be advantagous to override the default  <see cref="DefaultGraphQLHttpProcessor{TSchema}" /> and register your custom
        /// type here rather than overriding the entire graphql http middleware component. See the documentation for further details.</para>
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
                _httpProcessorType = value;
                this.TypeReferenceAdded?.Invoke(this, new TypeReferenceEventArgs(value, ServiceLifetime.Scoped));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the the default http processor
        /// will only accept authenticated requests.  When <c>true</c>, the graphql runtime will not be invoked
        /// if the requestor has not been authenticated by ASP.NET. (Default: false).
        /// </summary>
        /// <value><c>true</c> if only authorized requests should be handled by graphql; otherwise, <c>false</c>.</value>
        public bool AuthenticatedRequestsOnly { get; set; }
    }
}