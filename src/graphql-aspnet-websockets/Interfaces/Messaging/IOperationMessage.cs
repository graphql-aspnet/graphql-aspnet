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
    /// A representation of a graphql message sent or recieved over a presistent connection.
    /// </summary>
    public interface IOperationMessage
    {
        /// <summary>
        /// Gets or sets the payload of the message as a stringified json object.
        /// </summary>
        /// <value>The payload.</value>
        string Payload { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the scoped operation started by a client.
        /// </summary>
        /// <value>The identifier.</value>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the message, indicating expected payload types.
        /// </summary>
        /// <value>The type.</value>
        OperationMessageType Type { get; set; }
    }
}