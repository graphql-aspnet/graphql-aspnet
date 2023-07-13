﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ArrayReturnController : GraphController
    {
        [QueryRoot]
        public TwoPropertyObject[] RetrieveArray()
        {
            return new TwoPropertyObject[2]
            {
                new TwoPropertyObject() { Property1 = "1A", Property2 = 2, },
                new TwoPropertyObject() { Property1 = "1B", Property2 = 3, },
            };
        }
    }
}