// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Messaging
{
    using GraphQL.AspNet.Messaging;

    /// <summary>
    /// A base representation of a graphql message sent or recieved over a presistent connection. This interface does not
    /// include custom payload information.
    /// </summary>
    public interface IGraphQLOperationMessage
    {
        /// <summary>
        /// Gets or sets the identifier for the scoped operation started by a client.
        /// </summary>
        /// <value>The identifier.</value>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the message, indicating expected payload types.
        /// </summary>
        /// <value>The type.</value>
        ApolloMessageType Type { get; set; }

        /// <summary>
        /// Gets the payload of the message as a general object.
        /// </summary>
        /// <value>The payload object.</value>
        object PayloadObject { get; }
    }
}