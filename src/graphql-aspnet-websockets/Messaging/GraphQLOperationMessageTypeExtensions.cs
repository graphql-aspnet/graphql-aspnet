// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************


using System;

namespace GraphQL.AspNet.Messaging
{
    /// <summary>
    /// Helper methods for the <see cref="GraphQLOperationMessageType"/>.
    /// </summary>
    public static class GraphQLOperationMessageTypeExtensions
    {
        /// <summary>
        /// A helper message to create a valid <see cref="GraphQLOperationMessageType"/> accounting
        /// for various idiosyncrasies and possibilities of the message types as implemented by apollo.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>GraphQLOperationMessageType.</returns>
        public static GraphQLOperationMessageType FromString(string text)
        {
            if (text == null || text.Length == 0)
                return GraphQLOperationMessageType.UNKNOWN;

            text = text.ToLowerInvariant();

            // quirk of the apollo client. uses a shortened version of keep alive
            // for message transfer optimization
            // https://github.com/apollographql/subscriptions-transport-ws/blob/master/src/message-types.ts
            if (text == "ka")
                return GraphQLOperationMessageType.CONNECTION_KEEP_ALIVE;

            if (Enum.TryParse<GraphQLOperationMessageType>(text, true, out var result))
                return result;

            // just in case, by some random fluke, someone transmits a string with "GQL_" appended
            // thinking the static constant in apollo client is the connection type
            // handle it appropriately
            text = $"gql_{text}";
            if (Enum.TryParse<GraphQLOperationMessageType>(text, true, out result))
                return result;

            // dunno what the message type is
            return GraphQLOperationMessageType.UNKNOWN;
        }

        /// <summary>
        /// Serializes the specified message type to a string that is expected
        /// to be handled by downstream clients.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>System.String.</returns>
        public static string Serialize(GraphQLOperationMessageType messageType)
        {
            if (messageType == GraphQLOperationMessageType.CONNECTION_KEEP_ALIVE)
                return "ka";
            else
                return messageType.ToString().ToLowerInvariant();
        }
    }
}