﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages
{
    using System;

    /// <summary>
    /// Helper methods for the <see cref="GraphqlWsLegacyMessageType"/>.
    /// </summary>
    public static class GraphqlWsLegacyMessageTypeExtensions
    {
        /// <summary>
        /// A helper message to create a valid <see cref="GraphqlWsLegacyMessageType"/> accounting
        /// for various idiosyncrasies and possibilities of the message types as implemented by GraphqlWsLegacy.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>GraphQLOperationMessageType.</returns>
        public static GraphqlWsLegacyMessageType FromString(string text)
        {
            if (text == null || text.Length == 0)
                return GraphqlWsLegacyMessageType.UNKNOWN;

            text = text.ToLowerInvariant();

            // quirk of the GraphqlWsLegacy client. uses a shortened version of keep alive
            // for message transfer optimization
            // https://github.com/GraphqlWsLegacygraphql/subscriptions-transport-ws/blob/master/src/message-types.ts
            if (text == "ka")
                return GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE;

            if (Enum.TryParse<GraphqlWsLegacyMessageType>(text, true, out var result))
                return result;

            // just in case, by some random fluke, someone transmits a string with "GQL_" appended
            // thinking the static constant in GraphqlWsLegacy client is the connection type
            // handle it appropriately
            text = $"gql_{text}";
            if (Enum.TryParse(text, true, out result))
                return result;

            // dunno what the message type is
            return GraphqlWsLegacyMessageType.UNKNOWN;
        }

        /// <summary>
        /// Serializes the specified message type to a string that is expected
        /// to be handled by downstream clients.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>System.String.</returns>
        public static string Serialize(GraphqlWsLegacyMessageType messageType)
        {
            if (messageType == GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE)
                return "ka";
            else
                return messageType.ToString().ToLowerInvariant();
        }
    }
}