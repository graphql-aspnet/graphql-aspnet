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
    /// <summary>
    /// A keep alive message sent periodically by the server to keep the connection
    /// open a the application level.
    /// </summary>
    public class KeepAliveOperationMessage : GraphQLOperationMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeepAliveOperationMessage"/> class.
        /// </summary>
        public KeepAliveOperationMessage()
            : base(OperationMessageType.GQL_CONNECTION_KEEP_ALIVE)
        {
        }
    }
}