// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Web
{
    /// <summary>
    /// Indicates the message type recieved by a client connection.
    /// </summary>
    public enum ClientMessageType
    {
        /// <summary>
        /// The message type should be ignored by a client proxy. This status is primarly
        /// used by unit testing frameworks.
        /// </summary>
        Ignore = -1,

        /// <summary>
        /// The message data represents a clear text message.
        /// </summary>
        Text = 0,

        /// <summary>
        /// The message data represents a binary formatted message.
        /// </summary>
        Binary = 1,

        /// <summary>
        /// A message that indicates the client is closing the connection. The received data
        /// bits may or may not be useful.
        /// </summary>
        Close = 2,
    }
}