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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class ArrayOnReturnObjectMethodController : GraphController
    {
        [QueryRoot]
        public ArrayObject RetrieveData()
        {
            return
                new ArrayObject()
                {
                    PropertyA = "AA",
                };
        }

        public class ArrayObject
        {
            public string PropertyA { get; set; }

            [GraphField]
            public TwoPropertyObject[] MoreData()
            {
                return new TwoPropertyObject[2]
                    {
                        new TwoPropertyObject() { Property1 = "1A", Property2 = 2, },
                        new TwoPropertyObject() { Property1 = "1B", Property2 = 3, },
                    };
            }
        }
    }
}