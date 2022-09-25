// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;

    /// <summary>
    /// A handler used when subscribing to messages raised by a connected client using the
    /// graphql-ws protocol.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="GQLWSMessage" /> instance containing the event data.</param>
    /// <returns>Task.</returns>
    public delegate Task GQLWSMessageRecievedDelegate(object sender, GQLWSMessage e);
}