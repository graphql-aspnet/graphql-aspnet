// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Apollo
{
    /// <summary>
    /// A set of constants related to apollo server.
    /// </summary>
    public static class ApolloConstants
    {
        /// <summary>
        /// A set of constants relating to Apollo's messaging protocol.
        /// </summary>
        public static class Messaging
        {
            /// <summary>
            /// A constnat representing the "id" field of any outbound apollo message.
            /// </summary>
            public const string MESSAGE_ID = "id";

            /// <summary>
            /// A constnat representing the "typ" field of any outbound apollo message.
            /// </summary>
            public const string MESSAGE_TYPE = "type";

            /// <summary>
            /// A constnat representing the "payload" field of any outbound apollo message.
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