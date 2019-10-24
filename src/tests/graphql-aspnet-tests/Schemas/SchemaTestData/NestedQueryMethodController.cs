// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.CommonHelpers;

    [GraphRoute("path0")]
    public class NestedQueryMethodController : GraphController
    {
        [Query("path1/path2")]
        public TwoPropertyObjectV2 TestActionMethod(float arg1, DateTime arg2)
        {
            return new TwoPropertyObjectV2()
            {
                Property1 = arg1,
                Property2 = arg2,
            };
        }
    }
}