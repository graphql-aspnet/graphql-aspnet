// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Execution.SubscriptionQueryExecutionData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphRoute("subscriptionData")]
    public class SubQueryController : GraphController
    {
        [Subscription]
        public TwoPropertyObject RetrieveObject(TwoPropertyObject source)
        {
            return source;
        }

        [Query]
        public TwoPropertyObject QueryRetrieveObject()
        {
            return new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 5,
            };
        }
    }
}