// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.Subscriptions.Tests.Engine.TestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class SubscriptionTestController : GraphController
    {
        [Subscription]
        [ApplyDirective(typeof(DirectiveWithArgs), 99, "sub action arg")]
        public string DoSub(string sourceData)
        {
            return null;
        }
    }
}