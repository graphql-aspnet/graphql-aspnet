// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Configuration.ConfigurationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class FanController : GraphController
    {
        [SubscriptionRoot("RetrieveFan")]
        public FanItem RetrieveFan(FanItem item, string name)
        {
            return item;
        }
    }
}