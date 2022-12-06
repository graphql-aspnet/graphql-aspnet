// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Mock
{
    using GraphQL.AspNet.Web;

    /// <summary>
    /// A queueable message that, when encountered by <see cref="MockClientConnection"/>,
    /// will instruct the connection to initiate a "close" operation as if it were requested
    /// by the connected client. This message is not transmitted to a client proxy.
    /// </summary>
    public class MockClientRemoteCloseMessage : MockSocketMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockClientRemoteCloseMessage" /> class.
        /// </summary>
        /// <param name="closeStatus">The close status indicated by the "client".</param>
        /// <param name="closeDescription">The close description provided by the "client".</param>
        public MockClientRemoteCloseMessage(
                    ConnectionCloseStatus? closeStatus = null,
                    string closeDescription = null)
            : base(
                  new byte[0],
                  ClientMessageType.Close,
                  closeStatus ?? ConnectionCloseStatus.NormalClosure,
                  closeDescription)
        {
        }
    }
}