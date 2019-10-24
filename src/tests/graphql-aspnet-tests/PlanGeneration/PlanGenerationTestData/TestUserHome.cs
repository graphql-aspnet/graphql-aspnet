// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration.PlanGenerationTestData
{
    using GraphQL.AspNet.Attributes;

    public class TestUserHome
    {
        [GraphField]
        public TestUser User { get; set; }

        [GraphField]
        public int Id { get; set; }

        [GraphField]
        public string HouseName { get; set; }
    }
}