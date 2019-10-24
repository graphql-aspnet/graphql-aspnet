// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration.ComplexityTestData
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    [GraphRoute("complexityTest")]
    internal class ComplexityTestController : GraphController
    {
        [Query]
        public TripleNestedComplexityObject RetrieveTripleNestedObject()
        {
            return new TripleNestedComplexityObject()
            {
                Object2 = new NestedComplexityTestObject()
                {
                    Object1 = new ComplexityTestObject()
                    {
                        Property1 = 5,
                        Property2 = "John",
                    },
                    Property3 = "Bob",
                },
                Object3 = new ComplexityTestObject()
                {
                    Property1 = 1,
                    Property2 = "Jane",
                },
                Property4 = DateTime.UtcNow,
                Property5 = new GraphId("smith"),
            };
        }
    }
}