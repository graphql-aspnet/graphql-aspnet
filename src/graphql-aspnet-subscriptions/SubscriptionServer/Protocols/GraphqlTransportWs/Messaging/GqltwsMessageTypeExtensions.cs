﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging
{
    using System;

    /// <summary>
    /// Helper methods for the <see cref="GqltwsMessageType"/>.
    /// </summary>
    internal static class GqltwsMessageTypeExtensions
    {
        /// <summary>
        /// A helper message to create a valid <see cref="GqltwsMessageType"/> accounting
        /// for various idiosyncrasies and possibilities of the message types as implemented by graphql-ws.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>GraphQLOperationMessageType.</returns>
        public static GqltwsMessageType FromString(string text)
        {
            if (text == null || text.Length == 0)
                return GqltwsMessageType.UNKNOWN;

            text = text.ToLowerInvariant();

            if (Enum.TryParse<GqltwsMessageType>(text, true, out var result))
                return result;

            // just in case, by some random fluke, someone transmits a string with "GQL_" appended
            // thinking the static constant in graphql-ws client is the connection type
            // handle it appropriately
            text = $"gql_{text}";
            if (Enum.TryParse(text, true, out result))
                return result;

            // dunno what the message type is
            return GqltwsMessageType.UNKNOWN;
        }

        /// <summary>
        /// Serializes the specified message type to a string that is expected
        /// to be handled by downstream clients.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>System.String.</returns>
        public static string Serialize(GqltwsMessageType messageType)
        {
            return messageType.ToString().ToLowerInvariant();
        }
    }
}