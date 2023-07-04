// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.UnionTypeExecutionTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class UnrelatedItemsController : GraphController
    {

        [QueryRoot("retrieveItems", "TheThingsUnion", typeof(Television), typeof(Person), typeof(Home), TypeExpression = "[Type]")]
        public IGraphActionResult RetrieveItems()
        {
            var list = new List<object>();
            list.Add(new Person()
            {
                Id = 1,
                Name = "Bob Smith",
            });

            list.Add(new Television()
            {
                Name = "Tv 1",
            });

            list.Add(new Home()
            {
                Id = 1,
                Name = "Home 1",
                Width = 200,
                Height = 300,
            });

            list.Add(new Home()
            {
                Id = 2,
                Name = "Home 2",
                Width = 300,
                Height = 400,
            });

            return this.Ok(list);
        }

        [TypeExtension(typeof(IBuilding), "squareFeet", typeof(int))]
        public int BuildingSquareFeet(IBuilding building)
        {
            return building.Width * building.Height;
        }

        [BatchTypeExtension(typeof(IBuilding), "perimeter", typeof(int))]
        public IGraphActionResult BuildingPerimeter(IEnumerable<IBuilding> buildings)
        {
            var dic = new Dictionary<IBuilding, int>();

            foreach (var building in buildings)
                dic.Add(building, (building.Width * 2) + (building.Height * 2));

            return this.Ok(dic);
        }
    }
}