// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.FieldExecutionorderTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class HamburgerController : GraphController
    {
        [QueryRoot]
        public HamburgerMeal RetrieveDefaultHamburgerMeal()
        {
            return new HamburgerMeal()
            {
                Id = 5,
                Name = "Hamburger Supreme",
                Weight = 2.5f,
                Description = "a top level hamburger",
            };
        }

        [Query]
        public HamburgerMeal RetrieveOtherHamburgerMeal()
        {
            return new HamburgerMeal()
            {
                Id = 5,
                Name = "Tiny Hamburger",
                Weight = 1.3f,
                Description = "a nested Hamburger",
            };
        }
    }
}