// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Execution.Subscriptions.Apollo
{
    /// <summary>
    /// An event handler called when some status change occurs with a tracked event by the apollo server.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="ApolloTrackedEventArgs"/> instance containing the event data.</param>
    public delegate void ApolloTrackedEventChangeEventHandler(object sender, ApolloTrackedEventArgs args);
}