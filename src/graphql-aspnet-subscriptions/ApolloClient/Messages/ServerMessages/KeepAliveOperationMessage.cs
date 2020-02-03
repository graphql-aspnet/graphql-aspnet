// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging.ServerMessages
{
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Messaging.Messages.Payloads;

    /// <summary>
    /// A keep alive message sent periodically by the server to keep the connection
    /// open a the application level.
    /// </summary>
    public class KeepAliveOperationMessage : ApolloMessage<NullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeepAliveOperationMessage"/> class.
        /// </summary>
        public KeepAliveOperationMessage()
            : base(ApolloMessageType.CONNECTION_KEEP_ALIVE)
        {
        }
    }
}