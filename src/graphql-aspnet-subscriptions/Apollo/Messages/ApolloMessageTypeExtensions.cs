// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Messages
{
    using System;

    /// <summary>
    /// Helper methods for the <see cref="ApolloMessageType"/>.
    /// </summary>
    public static class ApolloMessageTypeExtensions
    {
        /// <summary>
        /// A helper message to create a valid <see cref="ApolloMessageType"/> accounting
        /// for various idiosyncrasies and possibilities of the message types as implemented by apollo.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>GraphQLOperationMessageType.</returns>
        public static ApolloMessageType FromString(string text)
        {
            if (text == null || text.Length == 0)
                return ApolloMessageType.UNKNOWN;

            text = text.ToLowerInvariant();

            // quirk of the apollo client. uses a shortened version of keep alive
            // for message transfer optimization
            // https://github.com/apollographql/subscriptions-transport-ws/blob/master/src/message-types.ts
            if (text == "ka")
                return ApolloMessageType.CONNECTION_KEEP_ALIVE;

            if (Enum.TryParse<ApolloMessageType>(text, true, out var result))
                return result;

            // just in case, by some random fluke, someone transmits a string with "GQL_" appended
            // thinking the static constant in apollo client is the connection type
            // handle it appropriately
            text = $"gql_{text}";
            if (Enum.TryParse(text, true, out result))
                return result;

            // dunno what the message type is
            return ApolloMessageType.UNKNOWN;
        }

        /// <summary>
        /// Serializes the specified message type to a string that is expected
        /// to be handled by downstream clients.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>System.String.</returns>
        public static string Serialize(ApolloMessageType messageType)
        {
            if (messageType == ApolloMessageType.CONNECTION_KEEP_ALIVE)
                return "ka";
            else
                return messageType.ToString().ToLowerInvariant();
        }
    }
}