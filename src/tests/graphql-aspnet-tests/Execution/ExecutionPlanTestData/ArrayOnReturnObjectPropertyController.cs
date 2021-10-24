// *************************************************************
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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class ArrayOnReturnObjectPropertyController : GraphController
    {
        [QueryRoot]
        public IEnumerable<ArrayObject> RetrieveData()
        {
            return new ArrayObject[1]
            {
                new ArrayObject()
                {
                    PropertyA = "AA",
                    PropertyB = new TwoPropertyObject[2]
                    {
                        new TwoPropertyObject() { Property1 = "1A", Property2 = 2, },
                        new TwoPropertyObject() { Property1 = "1B", Property2 = 3, },
                    },
                },
            };
        }

        public class ArrayObject
        {
            public string PropertyA { get; set; }

            public TwoPropertyObject[] PropertyB { get; set; }
        }
    }
}