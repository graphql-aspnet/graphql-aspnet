// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.SubscriptionEventLogEntries
{
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// A log event recorded when a subscription client attempts to connect with
    /// a messaging protocol not supported by the target schema.
    /// </summary>
    public class UnsupportClientSubscriptionProtocolLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportClientSubscriptionProtocolLogEntry" /> class.
        /// </summary>
        /// <param name="targetSchema">The target schema.</param>
        /// <param name="protocol">The unsupported protocol .</param>
        public UnsupportClientSubscriptionProtocolLogEntry(
            ISchema targetSchema,
            string protocol)
            : base(SubscriptionLogEventIds.UnsupportedClientProtocol)
        {
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
        /// Gets the client protocol(s) that were attempted but unsuccessfully negotiated.
        /// </summary>
        /// <value>The client protocol.</value>
        public string ClientProtocol
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.CLIENT_PROTOCOL);
            private set => this.SetProperty(SubscriptionLogPropertyNames.CLIENT_PROTOCOL, value);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Unsupported Subscription Client Protocol | Schema: {this.SchemaTypeName}, Protocol: {this.ClientProtocol}";
        }
    }
}