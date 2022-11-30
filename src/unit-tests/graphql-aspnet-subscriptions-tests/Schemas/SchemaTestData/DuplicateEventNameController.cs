// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Schemas.SchemaTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class DuplicateEventNameController : GraphController
    {
        [Subscription(EventName = "testEvent")]
        public TwoPropertyObject TestActionMethod(TwoPropertyObject sourceData, string arg1, int arg2)
        {
            return sourceData;
        }

        [Subscription(EventName = "testEvent")]
        public TwoPropertyObject TestActionMethod2(TwoPropertyObject sourceData, string arg1, int arg2)
        {
            return sourceData;
        }
    }
}