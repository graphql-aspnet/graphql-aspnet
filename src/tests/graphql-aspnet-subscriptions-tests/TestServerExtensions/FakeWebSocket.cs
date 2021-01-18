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
    using System;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class FakeWebSocket : WebSocket
    {
        private Exception _exceptionToThrowOnReceive;

        public FakeWebSocket(
            WebSocketState? state = null,
            WebSocketCloseStatus? closeStatus = null,
            string closeDescription = null,
            string subProtocol = null)
        {
            if (state.HasValue)
                this.State = state.Value;

            if (closeStatus.HasValue)
            {
                this.CloseStatus = closeStatus.Value;
                this.CloseStatusDescription = closeDescription;
            }

            this.SubProtocol = SubProtocol;
        }

        public void ThrowExceptionOnReceieve(Exception ex)
        {
            _exceptionToThrowOnReceive = ex;
        }

        public override void Abort()
        {
        }

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            this.TotalCloseCalls += 1;
            return Task.CompletedTask;
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
        }

        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            if (_exceptionToThrowOnReceive != null)
                throw _exceptionToThrowOnReceive;

            this.TotalCallsToReceive += 1;
            return Task.FromResult(new WebSocketReceiveResult(5, WebSocketMessageType.Text, true));
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            this.TotalCallsToSend += 1;
            return Task.CompletedTask;
        }

        public override WebSocketCloseStatus? CloseStatus { get; }

        public override string CloseStatusDescription { get; }

        public override WebSocketState State { get; }

        public override string SubProtocol { get; }

        public int TotalCallsToSend { get; private set; }

        public int TotalCallsToReceive { get; private set; }

        public int TotalCloseCalls { get; private set; }
    }
}