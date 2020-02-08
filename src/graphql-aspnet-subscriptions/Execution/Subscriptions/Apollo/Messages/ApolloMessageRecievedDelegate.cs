// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging
{
    using System.Threading.Tasks;

    /// <summary>
    /// A handler used when subscribing to messages raised by a connected client using the
    /// graphql over websocket protocol from Apollo.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ApolloMessage" /> instance containing the event data.</param>
    /// <returns>Task.</returns>
    public delegate Task ApolloMessageRecievedDelegate(object sender, ApolloMessage e);
}