// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging
{
    using System;

    /// <summary>
    /// A set of property names used by subscription log events.
    /// </summary>
    public static class SubscriptionLogPropertyNames
    {
        /// <summary>
        /// The subscription route assigned to a schema type when it was registered with ASP.NET.
        /// </summary>
        public const string SCHEMA_SUBSCRIPTION_ROUTE_PATH = "subscriptionRoute";

        /// <summary>
        /// The unique id assigned to the subscription client when it was first created
        /// by the configured subscription server.
        /// </summary>
        public const string SUBSCRIPTION_CLIENT_ID = "clientId";

        /// <summary>
        /// The unique id assigned to the subscription server when it was first created
        /// by the runtime.
        /// </summary>
        public const string SUBSCRIPTION_SERVER_ID = "serverId";

        /// <summary>
        /// The type name of the client that was instantiated by the subscription server.
        /// </summary>
        public const string SUBSCRIPTION_CLIENT_TYPE_NAME = "clientType";

        /// <summary>
        /// The type name of the server that was involved in the transaction.
        /// </summary>
        public const string SUBSCRIPTION_SERVER_TYPE_NAME = "subscriptionServerType";

        /// <summary>
        /// The <see cref="Type"/> name of the data object recevied with an event.
        /// </summary>
        public const string SUBSCRIPTION_EVENT_DATA_TYPE = "dataType";

        /// <summary>
        /// The qualified name a subscription event.
        /// </summary>
        public const string SUBSCRIPTION_EVENT_NAME = "subscriptionEventName";

        /// <summary>
        /// The unique id assigned to the subscription event when it was first raised
        /// at its source.
        /// </summary>
        public const string SUBSCRIPTION_EVENT_ID = "subscriptionEventId";

        /// <summary>
        /// The unique route within a schema that a subscription event is being reported against.
        /// </summary>
        public const string SUBSCRIPTION_PATH = "subscriptionPath";

        /// <summary>
        /// The physical name of the ASP.NET server hosting subscriptions.
        /// </summary>
        public const string ASPNET_SERVER_INSTANCE_NAME = "machineName";

        /// <summary>
        /// The total number of clients being reported on.
        /// </summary>
        public const string CLIENT_COUNT = "clientCount";

        /// <summary>
        /// The total number of subscriptions being reported on.
        /// </summary>
        public const string SUBSCRIPTION_COUNT = "subscriptionCount";

        /// <summary>
        /// A collection of client ids being reported on.
        /// </summary>
        public const string CLIENT_IDS = "clientIds";

        /// <summary>
        /// The name of a client messaging protocol.
        /// </summary>
        public const string CLIENT_PROTOCOL = "clientProtocol";

        /// <summary>
        /// The 'type' field of an message.
        /// </summary>
        public const string MESSAGE_TYPE = "messageType";

        /// <summary>
        /// The 'id' field of an message.
        /// </summary>
        public const string MESSAGE_ID = "messageId";

        /// <summary>
        /// The client supplied identifer assigned to the subscription.
        /// </summary>
        public const string SUBSCRIPTION_ID = "subscriptionId";

        /// <summary>
        /// A collection of subscription ids being reported on.
        /// </summary>
        public const string SUBSCRIPTION_IDS = "subscriptionIds";

        /// <summary>
        /// The specific event threshold, in number of events, that must be queued
        /// for the event to be raised.
        /// </summary>
        public const string SUBSCRIPTION_DISPATCH_QUEUE_THRESHOLD_LEVEL = "eventQueueThresholdLevel";

        /// <summary>
        /// The exact count of messages in the queue at the time the event was raised.
        /// </summary>
        public const string SUBSCRIPTION_DISPATCH_QUEUE_COUNT = "eventQueueCount";

        /// <summary>
        /// A custom configured message carried along with the raised event. May be null.
        /// </summary>
        public const string SUBSCRIPTION_DISPATCH_QUEUE_THRESHOLD_MESSAGE = "eventQueueThresholdMessage";
    }
}