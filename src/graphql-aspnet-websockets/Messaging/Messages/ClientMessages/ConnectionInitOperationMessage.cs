// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging.Messages
{
    using System.Diagnostics;
    using GraphQL.AspNet.Messaging.Messages.Payloads;

    /// <summary>
    /// A message recieved from the client after the establishment of the websocket to initialize the graphql
    /// session on the socket.
    /// </summary>
    [DebuggerDisplay("Client Init")]
    internal class ConnectionInitOperationMessage : GraphQLOperationMessage<NullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionInitOperationMessage"/> class.
        /// </summary>
        public ConnectionInitOperationMessage()
            : base(GraphQLOperationMessageType.CONNECTION_INIT)
        {
        }
    }
}