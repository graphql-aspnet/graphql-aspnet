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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class ChickenController : GraphController
    {
        [QueryRoot]
        public ChickenMeal RetrieveDefaultChickenMeal()
        {
            return new ChickenMeal()
            {
                Id = 5,
                Name = "Chicken Bacon Ranch",
                Description = "a top level chicken sandwich",
            };
        }

        [Query]
        public ChickenMeal RetrieveOtherChickenMeal()
        {
            return new ChickenMeal()
            {
                Id = 5,
                Name = "Chicken Lettuce Tomato",
                Description = "a nested chicken sandwich",
            };
        }
    }
}