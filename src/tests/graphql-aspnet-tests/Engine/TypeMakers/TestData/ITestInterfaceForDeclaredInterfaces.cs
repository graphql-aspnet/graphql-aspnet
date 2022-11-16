// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Engine.TypeMakers.TestData
{
    public interface ITestInterfaceForDeclaredInterfaces : ITestInterface1, ITestInterface2
    {
        public double Field3 { get; set; }
    }
}