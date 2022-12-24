// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlWsLegacy.GraphqlWsLegacyData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class GraphqlWsLegacySubscriptionController : GraphController
    {
        [Subscription]
        public TwoPropertyObject WatchForPropObject(TwoPropertyObject obj)
        {
            return obj;
        }

        [Subscription]
        public TwoPropertyObject WatchForPropObject2(TwoPropertyObject obj)
        {
            return obj;
        }

        [QueryRoot]
        public TwoPropertyObject FastQuery()
        {
            return new TwoPropertyObject()
            {
                Property1 = "bob",
                Property2 = 3,
            };
        }
    }
}