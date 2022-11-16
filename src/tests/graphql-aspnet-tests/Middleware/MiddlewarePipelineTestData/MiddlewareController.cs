// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Middleware.MiddlewarePipelineTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class MiddlewareController : GraphController
    {
        [Query]
        public string FieldOfData()
        {
            return "123";
        }
    }
}