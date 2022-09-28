// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Connections.Clients
{
    /// <summary>
    /// Indicates the message type recieved by a client connection.
    /// </summary>
    public enum ClientMessageType
    {
        /// <summary>
        /// The message is clear text.
        /// </summary>
        Text = 0,

        /// <summary>
        /// The message is in binary format.
        /// </summary>
        Binary = 1,

        /// <summary>
        /// A receive has completed because a close message was received.
        /// </summary>
        Close = 2,

        /// <summary>
        /// The message should be ignored by the client. This status is primarly
        /// used by unit testing frameworks.
        /// </summary>
        Ignore = 3,
    }
}