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
    public class NestedComplexityTestObject
    {
        public ComplexityTestObject Object1 { get; set; }

        public string Property3 { get; set; }
    }
}