// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.TestData
{
    using System.Collections.Generic;
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Tests.Mocks;
    using Microsoft.AspNetCore.Http;

    public class FakeWebSocketManager : WebSocketManager
    {
        public override bool IsWebSocketRequest => true;

        public override IList<string> WebSocketRequestedProtocols => new List<string>();

        public override Task<WebSocket> AcceptWebSocketAsync(string subProtocol)
        {
            return Task.FromResult(new FakeWebSocket(WebSocketState.Open) as WebSocket);
        }
    }
}