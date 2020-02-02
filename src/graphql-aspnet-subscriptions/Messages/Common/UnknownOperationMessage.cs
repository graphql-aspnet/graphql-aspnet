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
    using GraphQL.AspNet.Messaging.Messages.Payloads;

    /// <summary>
    /// A message representing an unknown message type.
    /// </summary>
    public class UnknownOperationMessage : GraphQLOperationMessage<NullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownOperationMessage"/> class.
        /// </summary>
        public UnknownOperationMessage()
            : base(GraphQLOperationMessageType.UNKNOWN)
        {
        }
    }
}