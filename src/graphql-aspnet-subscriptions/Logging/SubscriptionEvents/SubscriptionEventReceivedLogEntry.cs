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
    /// An event was recieved by the <see cref="ISubscriptionEventRouter"/> for this instance and will
    /// be sent to any requesting receivers.
    /// </summary>
    public class SubscriptionEventReceivedLogEntry : GraphLogEntry
    {
        private readonly string _eventName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventReceivedLogEntry" /> class.
        /// </summary>
        /// <param name="eventRecieved">The event that was recieved.</param>
        public SubscriptionEventReceivedLogEntry(SubscriptionEvent eventRecieved)
            : base(SubscriptionLogEventIds.GlobalEventReceived)
        {
            _eventName = eventRecieved.EventName;
            this.SchemaType = eventRecieved.SchemaTypeName;
            this.SubscriptionEventName = eventRecieved.EventName;
            this.DataType = eventRecieved.DataTypeName;
            this.SubscriptionEventId = eventRecieved.Id;
            this.MachineName = Environment.MachineName;
        }

        /// <summary>
        /// Gets the qualfied name of the schema type the event targets.
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
        /// Gets the unique id of the event that was received.
        /// </summary>
        /// <value>The identifier.</value>
        public string SubscriptionEventId
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_EVENT_ID);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_EVENT_ID, value);
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
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.SubscriptionEventId?.Length > 8 ? this.SubscriptionEventId.Substring(0, 8) : this.SubscriptionEventId;
            return $"Subscription Event Received | Server: {this.MachineName}, EventName: '{_eventName}' (Id: {idTruncated})";
        }
    }
}