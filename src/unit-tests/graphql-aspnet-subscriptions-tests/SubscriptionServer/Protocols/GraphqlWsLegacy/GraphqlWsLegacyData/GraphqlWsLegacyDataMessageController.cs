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

    public class GraphqlWsLegacyDataMessageController : GraphController
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