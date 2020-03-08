// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.Clients
{
    using GraphQL.AspNet.Connections.Clients;

    /// <summary>
    /// A messag that indicates the remote client has/is closing hte connection.
    /// </summary>
    public class MockClientRemoteCloseMessage : MockClientMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockClientRemoteCloseMessage" /> class.
        /// </summary>
        /// <param name="closeStatus">The close status.</param>
        /// <param name="closeDescription">The close description.</param>
        public MockClientRemoteCloseMessage(
                    ClientConnectionCloseStatus? closeStatus = null,
                    string closeDescription = null)
            : base(
                  new byte[0],
                  ClientMessageType.Close,
                  true,
                  closeStatus ?? ClientConnectionCloseStatus.NormalClosure,
                  closeDescription)
        {
        }
    }
}