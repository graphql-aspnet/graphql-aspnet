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
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphRoute("subscriptionData")]
    public class SubQueryController : GraphController
    {
        [Subscription]
        public TwoPropertyObject RetrieveObject(TwoPropertyObject source)
        {
            return source;
        }

        [QueryRoot(typeof(TwoPropertyObject))]
        public IGraphActionResult NormalQueryWithSkipEvent()
        {
            return this.SkipSubscriptionEvent();
        }

        [QueryRoot(typeof(TwoPropertyObject))]
        public IGraphActionResult NormalQueryWithComplete()
        {
            return this.OkAndComplete(new TwoPropertyObject());
        }

        [Subscription(typeof(TwoPropertyObject))]
        public IGraphActionResult SkipEventMethod(TwoPropertyObject source)
        {
            return this.SkipSubscriptionEvent();
        }

        [Subscription(typeof(TwoPropertyObject))]
        public IGraphActionResult SkipEventAndCompleteMethod(TwoPropertyObject source)
        {
            return this.SkipSubscriptionEvent(true);
        }

        [Subscription(typeof(TwoPropertyObject))]
        public IGraphActionResult CompleteMethod(TwoPropertyObject source)
        {
            return this.OkAndComplete(source);
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