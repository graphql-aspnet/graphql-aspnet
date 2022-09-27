// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy
{
    /// <summary>
    /// A set of constants related to GraphqlWsLegacy server.
    /// </summary>
    public static class GraphqlWsLegacyConstants
    {
        /// <summary>
        /// A legacy graphql over websocket messaging protcol originallly created by
        /// GraphqlWsLegacy.
        /// </summary>
        /// <remarks>
        /// Details: (https://github.com/GraphqlWsLegacygraphql/subscriptions-transport-ws/blob/master/PROTOCOL.md).
        /// </remarks>
        public const string PROTOCOL_NAME = "graphql-ws";

        /// <summary>
        /// An alternate name for the legacy 'graphql-ws' protocol transmitted by some
        /// tools.
        /// </summary>
        public const string ALTERNATE_PROTOCOL_NAME = "subscription-transport-ws";

        /// <summary>
        /// A set of constants relating to GraphqlWsLegacy's messaging protocol.
        /// </summary>
        public static class Messaging
        {
            /// <summary>
            /// A constnat representing the "id" field of any outbound GraphqlWsLegacy message.
            /// </summary>
            public const string MESSAGE_ID = "id";

            /// <summary>
            /// A constnat representing the "typ" field of any outbound GraphqlWsLegacy message.
            /// </summary>
            public const string MESSAGE_TYPE = "type";

            /// <summary>
            /// A constnat representing the "payload" field of any outbound GraphqlWsLegacy message.
            /// </summary>
            public const string MESSAGE_PAYLOAD = "payload";

            /// <summary>
            /// A constant representing the key, in a meta data collection on an error, of the id of message
            /// which generated the error.
            /// </summary>
            public const string LAST_RECEIVED_MESSAGE_ID = "lastMessage_id";

            /// <summary>
            /// A constant representing the key, in a meta data collection on an error, of the type of message
            /// which generated the error.
            /// </summary>
            public const string LAST_RECEIVED_MESSAGE_TYPE = "lastMessage_type";
        }
    }
}