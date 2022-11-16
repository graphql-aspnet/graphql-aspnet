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
    using System;
    using GraphQL.AspNet.Attributes;

    public class TestUser
    {
        [GraphField]
        public DateTime BirthDay { get; set; }

        [GraphField]
        public string Name { get; set; }

        [GraphField]
        public string Location { get; set; }
    }
}