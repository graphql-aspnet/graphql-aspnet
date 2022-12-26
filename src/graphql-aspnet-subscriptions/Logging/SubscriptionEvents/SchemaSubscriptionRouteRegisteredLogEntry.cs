﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.SubscriptionEvents
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Recorded when the startup services registers a publically available ASP.NET route to which
    /// end users can intiate a websocket request through which subscriptions can be established.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema for which the route was registered.</typeparam>
    public class SchemaSubscriptionRouteRegisteredLogEntry<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        private readonly string _schemaTypeShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaSubscriptionRouteRegisteredLogEntry{TSchema}" /> class.
        /// </summary>
        /// <param name="routePath">The route string (e.g. '/graphql').</param>
        public SchemaSubscriptionRouteRegisteredLogEntry(string routePath)
            : base(SubscriptionLogEventIds.SchemaRouteRegistered)
        {
            _schemaTypeShortName = typeof(TSchema).FriendlyName();
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.SchemaSubscriptionRoutePath = routePath;
        }

        /// <summary>
        /// Gets the <see cref="Type" /> name of the schema instance the pipeline was generated for.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SchemaTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the relative url registered for this schema type to listen on.
        /// </summary>
        /// <value>The name of the schema route.</value>
        public string SchemaSubscriptionRoutePath
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SCHEMA_SUBSCRIPTION_ROUTE_PATH);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SCHEMA_SUBSCRIPTION_ROUTE_PATH, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Subscription Route Registered | Schema Type: '{_schemaTypeShortName}', Route: '{this.SchemaSubscriptionRoutePath}' ";
        }
    }
}