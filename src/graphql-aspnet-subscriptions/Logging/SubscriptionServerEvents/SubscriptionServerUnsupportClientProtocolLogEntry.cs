﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.SubscriptionServerEvents
{
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// A log event recorded when a subscription client attempts to connect with
    /// a messaging protocol not supported by the target schema.
    /// </summary>
    public class SubscriptionServerUnsupportClientProtocolLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionServerUnsupportClientProtocolLogEntry" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="targetSchema">The target schema.</param>
        /// <param name="protocol">The unsupported protocol .</param>
        public SubscriptionServerUnsupportClientProtocolLogEntry(
            ISubscriptionServer server,
            ISchema targetSchema,
            string protocol)
            : base(SubscriptionLogEventIds.UnsupportedClientProtocol)
        {
            this.ServerId = server?.Id;
            this.SchemaTypeName = targetSchema?.Name;
            this.ClientProtocol = protocol;
        }

        /// <summary>
        /// Gets the qualified name of the schema this log entry is reported against.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SchemaTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the qualified name of the schema this log entry is reported against.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string ClientProtocol
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.CLIENT_PROTOCOL);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Subscription Server Unsupported Client Protocol | Schema: {this.SchemaTypeName}, Protocol: {this.ClientProtocol}";
        }
    }
}