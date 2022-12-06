// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.InheritanceTestData
{
    public interface ISquare
    {
        public string Length { get; set; }

        public string Width { get; set; }
    }
}