﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class ArrayAsEnumerableController : GraphIdController
    {
        [QueryRoot]
        public IEnumerable<TwoPropertyObject> RetrieveData()
        {
            return new TwoPropertyObject[2]
            {
                new TwoPropertyObject() { Property1 = "1A", Property2 = 2, },
                new TwoPropertyObject() { Property1 = "1B", Property2 = 3, },
            };
        }
    }
}