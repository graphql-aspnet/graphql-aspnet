// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages
{
    using System;

    /// <summary>
    /// Helper methods for the <see cref="GQLWSMessageType"/>.
    /// </summary>
    internal static class GQLWSMessageTypeExtensions
    {
        /// <summary>
        /// A helper message to create a valid <see cref="GQLWSMessageType"/> accounting
        /// for various idiosyncrasies and possibilities of the message types as implemented by graphql-ws.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>GraphQLOperationMessageType.</returns>
        public static GQLWSMessageType FromString(string text)
        {
            if (text == null || text.Length == 0)
                return GQLWSMessageType.UNKNOWN;

            text = text.ToLowerInvariant();

            if (Enum.TryParse<GQLWSMessageType>(text, true, out var result))
                return result;

            // just in case, by some random fluke, someone transmits a string with "GQL_" appended
            // thinking the static constant in graphql-ws client is the connection type
            // handle it appropriately
            text = $"gql_{text}";
            if (Enum.TryParse(text, true, out result))
                return result;

            // dunno what the message type is
            return GQLWSMessageType.UNKNOWN;
        }

        /// <summary>
        /// Serializes the specified message type to a string that is expected
        /// to be handled by downstream clients.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>System.String.</returns>
        public static string Serialize(GQLWSMessageType messageType)
        {
            return messageType.ToString().ToLowerInvariant();
        }
    }
}