// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionDirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class DirectiveTestController : GraphController
    {
        [QueryRoot]
        public TwoPropertyObject RetrieveObject()
        {
            return new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 2,
            };
        }
    }
}