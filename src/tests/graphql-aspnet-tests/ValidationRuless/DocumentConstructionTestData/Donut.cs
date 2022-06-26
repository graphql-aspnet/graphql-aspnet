// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration.DocumentConstructionTestData
{
    public class Donut
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DonutFlavor Flavor { get; set; }
    }
}