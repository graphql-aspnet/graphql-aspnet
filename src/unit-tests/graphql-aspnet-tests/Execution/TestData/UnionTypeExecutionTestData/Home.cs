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
    public class Home : IBuilding
    {
        public string Name { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int Id { get; set; }
    }
}