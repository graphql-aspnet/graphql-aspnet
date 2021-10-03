// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Controllers.ControllerTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphRoute("invoke")]
    public class InvokableController : GraphController
    {
        public InvokableController()
        {
        }

        [Mutation(typeof(string))]
        public Task<IGraphActionResult> MutationRaisesSubEvent(string arg1 = "default")
        {
            this.PublishSubscriptionEvent("event1", new TwoPropertyObject()
            {
                Property1 = arg1,
            });

            return Task.FromResult(this.Ok("data result"));
        }

        [Mutation(typeof(string))]
        public Task<IGraphActionResult> MutationRaisesSubEventNoData(string arg1 = "default")
        {
            this.PublishSubscriptionEvent("event1", null);
            return Task.FromResult(this.Ok("data result"));
        }

        [Mutation(typeof(string))]
        public Task<IGraphActionResult> MutationRaiseSubEventWithNoEventName(string arg1 = "default")
        {
            this.PublishSubscriptionEvent(null, new TwoPropertyObject()
            {
                Property1 = arg1,
            });

            return Task.FromResult(this.Ok("data result"));
        }
    }
}