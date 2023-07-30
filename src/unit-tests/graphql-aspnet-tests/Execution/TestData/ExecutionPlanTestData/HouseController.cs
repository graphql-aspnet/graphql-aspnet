// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData
{
    using GraphQL.AspNet.Attributes;

    [GraphRoute("houseFloorPlan")]
    public class HouseController : RoomController
    {
        [Query]
        public int TotalFloors()
        {
            return 2;
        }
    }
}