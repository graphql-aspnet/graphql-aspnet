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
    public class Bagel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsHot { get; set; }

        public int? OrderCreated { get; set; }
    }
}