// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.DirectiveProcessorTypeSystemLocationTestData
{
    using GraphQL.AspNet.Attributes;

    [ApplyDirective(typeof(LocationTestDirective))]
    public interface InterfaceTestObject
    {
        int Property1 { get; set; }
    }
}