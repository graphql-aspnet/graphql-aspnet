﻿// *************************************************************
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
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for working with web sockets.
    /// </summary>
    public static class WebSocketExtensions
    {
        /// <summary>
        /// A helper method to garuntee receipt of a full message off the websocket. This method accounts
        /// for the quirkyness and semi-stream behavior of the websocket protocol.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="messageReceiveBufferSize">Size of the message receive buffer.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task&lt;System.ValueTuple&lt;WebSocketReceiveResult, IEnumerable&lt;System.Byte&gt;&gt;&gt;.</returns>
        public static async Task<(WebSocketReceiveResult, IEnumerable<byte>)> ReceiveFullMessage(this WebSocket socket, int messageReceiveBufferSize = 4096, CancellationToken cancelToken = default(CancellationToken))
        {
            Validation.ThrowIfNull(socket, nameof(socket));

            WebSocketReceiveResult response;
            var message = new List<byte>();

            var buffer = new byte[messageReceiveBufferSize];
            do
            {
                response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken);
                message.AddRange(new ArraySegment<byte>(buffer, 0, response.Count));
            }
            while (!response.EndOfMessage);

            return (response, message);
        }
    }
}