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

    public class TripleNestedComplexityObject
    {
        public NestedComplexityTestObject Object2 { get; set; }

        public ComplexityTestObject Object3 { get; set; }

        public DateTime Property4 { get; set; }

        public GraphId Property5 { get; set; }
    }
}