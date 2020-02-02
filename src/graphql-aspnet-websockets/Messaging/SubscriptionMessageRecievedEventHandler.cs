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
    /// <summary>
    /// A handler used when subscribing to messages raised by a connected client using the
    /// graphql over websocket protocol from Apollo.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SubscriptionMessageReceivedEventArgs"/> instance containing the event data.</param>
    public delegate void SubscriptionMessageRecievedEventHandler(object sender, SubscriptionMessageReceivedEventArgs e);
}