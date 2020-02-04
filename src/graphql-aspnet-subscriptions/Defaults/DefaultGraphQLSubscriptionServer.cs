// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// The default implementation of graphql aspnet subscription server.
    /// </summary>
    public class DefaultGraphQLSubscriptionServer : ISubscriptionServer
    {
        public void Publish(string eventName, object data)
        {
            throw new System.NotImplementedException();
        }

        public void Register()
        {
            throw new System.NotImplementedException();
        }
    }
}