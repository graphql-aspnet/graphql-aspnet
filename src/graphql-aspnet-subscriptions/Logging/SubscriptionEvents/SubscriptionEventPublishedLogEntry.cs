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
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// An subscription event was recieved and successfully published from the internal event queue
    /// to the configured <see cref="ISubscriptionEventPublisher"/>.
    /// </summary>
    public class SubscriptionEventPublishedLogEntry : GraphLogEntry
    {
        private readonly string _shortEventName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventPublishedLogEntry" /> class.
        /// </summary>
        /// <param name="eventPublished">The event that was recieved.</param>
        public SubscriptionEventPublishedLogEntry(SubscriptionEvent eventPublished)
            : base(SubscriptionLogEventIds.GlobalEventPublished)
        {
            _shortEventName = eventPublished.EventName;
            this.SchemaType = eventPublished.SchemaTypeName;
            this.DataType = eventPublished.DataTypeName;
            this.SubscriptionEventId = eventPublished.Id;
            this.SubscriptionEventName = eventPublished.EventName;
            this.MachineName = Environment.MachineName;
        }

        /// <summary>
        /// Gets the qualified name of the event that was recieved.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SchemaType
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the <see cref="Type"/> name of the data object received witht he event.
        /// </summary>
        /// <value>The type name of the data object.</value>
        public string DataType
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_EVENT_DATA_TYPE);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_EVENT_DATA_TYPE, value);
        }

        /// <summary>
        /// Gets the qualfied name of the schema type the event targets.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SubscriptionEventName
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_EVENT_NAME);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_EVENT_NAME, value);
        }

        /// <summary>
        /// Gets the name of the physical machine that generated this log entry.
        /// </summary>
        /// <value>The server name of the computer hosting the subscription service.</value>
        public string MachineName
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.ASPNET_SERVER_INSTANCE_NAME);
            private set => this.SetProperty(SubscriptionLogPropertyNames.ASPNET_SERVER_INSTANCE_NAME, value);
        }

        /// <summary>
        /// Gets the unique id of the event that was received.
        /// </summary>
        /// <value>The identifier.</value>
        public string SubscriptionEventId
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_EVENT_ID);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_EVENT_ID, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.SubscriptionEventId?.Length > 8 ? this.SubscriptionEventId.Substring(0, 8) : this.SubscriptionEventId;
            return $"Subscription Event Published | Server: {this.MachineName}, EventName: '{_shortEventName}' (Id: {idTruncated})";
        }
    }
}