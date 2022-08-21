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

    public enum EnumValueTestObject
    {
        [ApplyDirective(typeof(LocationTestDirective))]
        Value1,
        Value2,
    }
}