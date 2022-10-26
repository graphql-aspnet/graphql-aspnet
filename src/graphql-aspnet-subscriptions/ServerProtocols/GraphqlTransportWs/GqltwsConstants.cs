// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs
{
    /// <summary>
    /// A set of constants related to the graphql-ws server.
    /// </summary>
    internal static class GqltwsConstants
    {
        /// <summary>
        /// A graphql over websocket messaging protcol.
        /// </summary>
        /// <remarks>
        /// Details: (https://github.com/enisdenjo/graphql-ws/blob/master/PROTOCOL.md).
        /// </remarks>
        public const string PROTOCOL_NAME = "graphql-transport-ws";

        /// <summary>
        /// A set of constants relating to graphql-ws messaging protocol.
        /// </summary>
        public static class Messaging
        {
            /// <summary>
            /// A constant representing the "id" field of any outbound graphql-ws message.
            /// </summary>
            public const string MESSAGE_ID = "id";

            /// <summary>
            /// A constant representing the "typ" field of any outbound graphql-ws message.
            /// </summary>
            public const string MESSAGE_TYPE = "type";

            /// <summary>
            /// A constant representing the "payload" field of any outbound graphql-ws message.
            /// </summary>
            public const string MESSAGE_PAYLOAD = "payload";

            /// <summary>
            /// A conconstantstant representing the key, in a meta data collection on an error, of the id of message
            /// which generated the error.
            /// </summary>
            public const string LAST_RECEIVED_MESSAGE_ID = "lastMessage_id";

            /// <summary>
            /// A constant representing the key, in a meta data collection on an error, of the type of message
            /// which generated the error.
            /// </summary>
            public const string LAST_RECEIVED_MESSAGE_TYPE = "lastMessage_type";
        }

        /// <summary>
        /// A set of ids detailing custom codes used during the closing of a client.
        /// </summary>
        public static class CustomCloseEventIds
        {
            /// <summary>
            /// A web socket closing code indicating the server received a message it didn't
            /// understand.
            /// </summary>
            public const ushort InvalidMessageType = 4400;

            /// <summary>
            /// A web socket closing code indicating the server did not receive
            /// the required connection init method within the expected amount of time.
            /// </summary>
            public const ushort ConnectionInitializationTimeout = 4408;

            /// <summary>
            /// A web socket closing code indicating the server received too many
            /// initialization messages from the connected client.
            /// </summary>
            public const ushort TooManyInitializationRequests = 4429;
        }
    }
}