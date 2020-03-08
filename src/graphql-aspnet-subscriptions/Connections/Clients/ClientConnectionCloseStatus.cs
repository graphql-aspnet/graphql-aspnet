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
    /// Possible reasons why the client connection was closed.
    /// </summary>
    public enum ClientConnectionCloseStatus
    {
        /// <summary>
        /// An unknown, unused status
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The connection has closed after the request was fulfilled.
        /// </summary>
        NormalClosure = 1000,

        /// <summary>
        /// Indicates an endpoint is being removed. Either the server or client will
        /// become unavailable.
        /// </summary>
        EndpointUnavailable = 1001,

        /// <summary>
        /// The client or server is terminating the connection because of a protocol
        /// error.
        /// </summary>
        ProtocolError = 1002,

        ///
        /// <summary>
        /// The client or server is terminating the connection because it cannot accept
        /// the data type it received.
        /// </summary>
        InvalidMessageType = 1003,

        /// <summary>
        /// No error specified.
        /// </summary>
        Empty = 1005,

        /// <summary>
        /// The client or server is terminating the connection because it has received
        /// data inconsistent with the message type.
        /// </summary>
        InvalidPayloadData = 1007,

        /// <summary>
        /// The connection will be closed because an endpoint has received a message
        /// that violates its policy.
        /// </summary>
        PolicyViolation = 1008,

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        MessageTooBig = 1009,

        /// <summary>
        /// The client is terminating the connection because it expected the server
        /// to negotiate an extension.
        /// </summary>
        MandatoryExtension = 1010,

        /// <summary>
        /// The connection will be closed by the server because of an error on the server.
        /// </summary>
        InternalServerError = 1011,
    }
}