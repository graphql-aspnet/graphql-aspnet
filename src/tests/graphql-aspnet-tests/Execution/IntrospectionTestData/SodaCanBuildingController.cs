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
    using GraphQL.AspNet.Controllers;

    [GraphRoute("buildings")]
    public class SodaCanBuildingController : GraphController
    {
        [Query]
        public int AddNewBuilding(SodaCanBuildingData building)
        {
            return 0;
        }
    }
}