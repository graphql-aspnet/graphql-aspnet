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
    using System;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A result issued by a client when the receive operation fails to complete
    /// successfully or as intended, likely due to an exception being thrown.
    /// </summary>
    public class ClientConnectionFailureResult : IClientConnectionReceiveResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnectionFailureResult" /> class.
        /// </summary>
        /// <param name="exception">The exception  thrown to cause the failure, if any.</param>
        /// <param name="closeMessage">The close status message to apply to the result.</param>
        public ClientConnectionFailureResult(Exception exception = null, string closeMessage = null)
        {
            this.Exception = exception;
            this.CloseStatusDescription = closeMessage?.Trim();
            this.CloseStatus = ConnectionCloseStatus.InternalServerError;
            this.Count = 0;
            this.MessageType = ClientMessageType.Close;
        }

        /// <inheritdoc />
        public ConnectionCloseStatus? CloseStatus { get; }

        /// <inheritdoc />
        public string CloseStatusDescription { get; }

        /// <inheritdoc />
        public int Count { get; }

        /// <inheritdoc />
        public ClientMessageType MessageType { get; }

        /// <summary>
        /// Gets the exception thrown that caused this failure result, if any.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; }
    }
}