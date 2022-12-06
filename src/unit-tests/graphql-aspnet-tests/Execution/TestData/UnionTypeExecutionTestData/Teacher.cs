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
    public class Teacher : Person
    {
        public int NumberOfStudents { get; set; }
    }
}