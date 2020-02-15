// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Execution.Subscriptions
{
    /// <summary>
    /// A delegate used to raise events related to a <see cref="SubscriptionEvent"/>.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="SubscriptionEventEventArgs"/> instance containing the event data.</param>
    public delegate void SubscriptionEventHandler(object sender, SubscriptionEventEventArgs args);
}