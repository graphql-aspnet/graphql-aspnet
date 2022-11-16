// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.IntrospectionTestData
{
    using GraphQL.AspNet.Attributes;

    [GraphType(InputName = "BuildingInput")]
    public class SodaCanBuildingData
    {
        [GraphField]
        public string Name { get; set; }

        [GraphField]
        public string Address { get; set; }

        [GraphField]
        public CapacityType Capacity { get; set; }
    }
}