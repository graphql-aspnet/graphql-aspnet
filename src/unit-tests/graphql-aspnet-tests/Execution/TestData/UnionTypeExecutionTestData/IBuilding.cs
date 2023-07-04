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
    public interface IBuilding
    {
        int Id { get; }

        int Width { get; set; }

        int Height { get; set; }
    }
}