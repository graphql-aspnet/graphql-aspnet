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

    /// <summary>
    /// A message sent by the client when it wants to start a new subscription operation.
    /// </summary>
    [DebuggerDisplay("Client Start (Id: {Id})")]
    public class StartOperationMessage : ApolloMessage<GraphQueryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartOperationMessage"/> class.
        /// </summary>
        public StartOperationMessage()
            : base(ApolloMessageType.START)
        {
        }
    }
}