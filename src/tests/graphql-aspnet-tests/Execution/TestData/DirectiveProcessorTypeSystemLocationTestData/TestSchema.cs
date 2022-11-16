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
    using GraphQL.AspNet.Schemas;

    [ApplyDirective(typeof(LocationTestDirective))]
    internal class TestSchema : GraphSchema
    {
    }
}