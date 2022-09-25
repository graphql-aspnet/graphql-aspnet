// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.TestServerExtensions
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Connections.WebSockets;
    using Microsoft.AspNetCore.Http;

    public class FakeableWebSocketClientConnection : WebSocketClientConnection
    {
        private readonly FakeWebSocket _fakeSocket;

        public FakeableWebSocketClientConnection(FakeWebSocket websocket, HttpContext context)
            : base(context)
        {
            _fakeSocket = websocket;
        }

        public override Task OpenAsync(string protocol)
        {
            this.WebSocket = _fakeSocket;
            return Task.CompletedTask;
        }
    }
}