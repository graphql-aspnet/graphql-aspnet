// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common;

    /// <summary>
    /// This object wraps a connected websocket to characterize it and provide
    /// subscription support for the graphql-ws protocol.
    /// </summary>
    internal abstract class GqltwsClientProxy
    {
        /// <summary>
        /// Sends the given message down the wire to the connected client.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        public abstract Task SendMessage(GqltwsMessage message);

        /// <summary>
        /// Gets the current state of the underlying connection.
        /// </summary>
        /// <value>The state.</value>
        public abstract ClientConnectionState State { get; }
    }
}