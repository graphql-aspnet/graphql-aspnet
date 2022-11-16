// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.TypeSystemDirectiveInvocationTestData
{
    using GraphQL.AspNet.Attributes;

    [ApplyDirective(typeof(ObjectDirectiveToInvoke))]
    public class ObjectForDirectiveInvocation
    {
        public int Property { get; set; }
    }
}