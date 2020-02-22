// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Apollo.ApolloTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class ApolloDataMessageController : GraphController
    {
        [QueryRoot]
        public TwoPropertyObject GetValue()
        {
            return new TwoPropertyObject()
            {
                Property1 = "abc123",
                Property2 = 15,
            };
        }
    }
}