// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData
{
    public interface IInterfaceB : IInterfaceA
    {
        public string InterfaceBField { get; set; }
    }
}