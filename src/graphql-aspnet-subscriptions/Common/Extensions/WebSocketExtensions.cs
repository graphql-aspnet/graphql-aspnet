// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// Extension methods for working with web sockets.
    /// </summary>
    public static class WebSocketExtensions
    {
        /// <summary>
        /// A helper method to guarantee receipt of a full message off the websocket. This method accounts
        /// for the quirkyness and semi-stream behavior of the websocket protocol.
        /// </summary>
        /// <param name="connection">The connection to receive teh message from.</param>
        /// <param name="messageReceiveBufferSize">Size of the message receive buffer.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task&lt;System.ValueTuple&lt;WebSocketReceiveResult, IEnumerable&lt;System.Byte&gt;&gt;&gt;.</returns>
        public static async Task<(IClientConnectionReceiveResult, IEnumerable<byte>)> ReceiveFullMessage(
            this IClientConnection connection,
            int messageReceiveBufferSize = 4096,
            CancellationToken cancelToken = default(CancellationToken))
        {
            Validation.ThrowIfNull(connection, nameof(connection));

            IClientConnectionReceiveResult response;
            var message = new List<byte>();

            var buffer = new byte[messageReceiveBufferSize];
            do
            {
                response = await connection.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken);
                message.AddRange(new ArraySegment<byte>(buffer, 0, response.Count));
            }
            while (!response.EndOfMessage);

            return (response, message);
        }
    }
}