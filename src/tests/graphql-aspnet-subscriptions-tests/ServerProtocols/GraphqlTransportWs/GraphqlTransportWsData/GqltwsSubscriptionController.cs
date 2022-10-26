// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs.GraphqlTransportWsData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class GqltwsSubscriptionController : GraphController
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

        [Subscription(typeof(TwoPropertyObject))]
        public IGraphActionResult WatchForPropObjectAndComplete(TwoPropertyObject obj)
        {
            return this.OkAndComplete(obj);
        }

        [Subscription(typeof(TwoPropertyObject))]
        public IGraphActionResult WatchForPropObjectSkipAndComplete(TwoPropertyObject obj)
        {
            return this.SkipSubscriptionEvent(true);
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