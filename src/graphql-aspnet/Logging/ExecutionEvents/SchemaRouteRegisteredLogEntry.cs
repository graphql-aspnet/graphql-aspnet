// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ExecutionEvents
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when the startup services registers a publically available ASP.NET MVC route to which
    /// end users can submit graphql queries.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema for which the route was registered.</typeparam>
    public class SchemaRouteRegisteredLogEntry<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        private readonly string _schemaTypeShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaRouteRegisteredLogEntry{TSchema}" /> class.
        /// </summary>
        /// <param name="routePath">The route string (e.g. '/graphql').</param>
        public SchemaRouteRegisteredLogEntry(string routePath)
            : base(LogEventIds.SchemaRouteRegistered)
        {
            _schemaTypeShortName = typeof(TSchema).FriendlyName();
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.SchemaRoutePath = routePath;
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
        public string SchemaRoutePath
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_ROUTE_PATH);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_ROUTE_PATH, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Schema Route Registered | Schema Type: '{_schemaTypeShortName}', Route: '{this.SchemaRoutePath}' ";
        }
    }
}