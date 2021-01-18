// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Logging.ApolloEvents
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when an Apollo server component unregistered a subscription event on the configured
    /// listener for this ASP.NET server instance. This log entry is recorded when the last connected client
    /// stops its last subscription for a given event.
    /// </summary>
    /// <typeparam name="TSchema">The type of schema the event is being raised against.</typeparam>
    public class ApolloServerEventMonitorEndedLogEntry<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        private readonly string _schemaTypeShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloServerEventMonitorEndedLogEntry{TSchema}" /> class.
        /// </summary>
        /// <param name="server">The server that registered with the listener.</param>
        /// <param name="eventName">Name of the event that is no longer being monitored.</param>
        public ApolloServerEventMonitorEndedLogEntry(
            ApolloSubscriptionServer<TSchema> server,
            SubscriptionEventName eventName)
            : base(ApolloLogEventIds.ServerSubscriptionEventMonitorStarted)
        {
            _schemaTypeShortName = typeof(TSchema).FriendlyName();
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.SubscriptionEventName = eventName.ToString();
            this.ServerId = server.Id;
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
        /// Gets the <see cref="Type" /> name of the schema instance the pipeline was generated for.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SchemaTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the name of the event that was included in this log entry.
        /// </summary>
        /// <value>The name of the event.</value>
        public string SubscriptionEventName
        {
            get => this.GetProperty<string>(ApolloLogPropertyNames.SUBSCRIPTION_EVENT_NAME);
            private set => this.SetProperty(ApolloLogPropertyNames.SUBSCRIPTION_EVENT_NAME, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Server Event Monitor Stopped | Schema: {_schemaTypeShortName}, Event: {this.SubscriptionEventName}";
        }
    }
}