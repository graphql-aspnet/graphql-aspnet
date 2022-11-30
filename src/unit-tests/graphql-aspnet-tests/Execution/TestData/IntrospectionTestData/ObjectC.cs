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
    public class ObjectC : IInterfaceB
    {
        public string ObjectCField { get; set; }

        public string InterfaceBField { get; set; }

        public string InterfaceAField { get; set; }
    }
}