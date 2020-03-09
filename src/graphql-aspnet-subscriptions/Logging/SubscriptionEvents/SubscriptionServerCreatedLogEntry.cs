// *************************************************************
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
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when a new subscription server is created by the runtime.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema for the server was created.</typeparam>
    public class SubscriptionServerCreatedLogEntry<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        private readonly string _serverTypeShortName;
        private readonly string _schemaTypeShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionServerCreatedLogEntry{TSchema}" /> class.
        /// </summary>
        /// <param name="server">The server which was created.</param>
        public SubscriptionServerCreatedLogEntry(ISubscriptionServer<TSchema> server)
            : base(SubscriptionLogEventIds.SubscriptionServerCreated)
        {
            _serverTypeShortName = server.GetType().FriendlyName();
            _schemaTypeShortName = typeof(TSchema).FriendlyName();
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.ServerTypeName = server.GetType().FriendlyName(true);
            this.ServerId = server.Id;
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
        /// Gets the <see cref="Type"/> name of the subscription server that created the client.
        /// </summary>
        /// <value>The name of the server type.</value>
        public string ServerTypeName
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_SERVER_TYPE_NAME);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_SERVER_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the unique id of the client that was created.
        /// </summary>
        /// <value>The identifier.</value>
        public string ServerId
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_SERVER_ID);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_SERVER_ID, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.ServerId?.Length > 8 ? this.ServerId.Substring(0, 8) : this.ServerId;
            return $"New Subscription Server Registered | Schema: '{_schemaTypeShortName}', Type: '{_serverTypeShortName}' (Id: {idTruncated})";
        }
    }
}