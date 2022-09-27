// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.GraphqlWsLegacy.Messages
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.Common;

    /// <summary>
    /// A handler used when subscribing to messages raised by a connected client using the
    /// graphql over websocket protocol from GraphqlWsLegacy.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="GraphqlWsLegacyMessage" /> instance containing the event data.</param>
    /// <returns>Task.</returns>
    public delegate Task GraphqlWsLegacyMessageRecievedDelegate(object sender, GraphqlWsLegacyMessage e);
}