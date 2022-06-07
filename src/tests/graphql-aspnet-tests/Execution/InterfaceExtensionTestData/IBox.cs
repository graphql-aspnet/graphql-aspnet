// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.InterfaceExtensionTestData
{
    public interface IBox : ISquare
    {
        public string Height { get; set; }
    }
}