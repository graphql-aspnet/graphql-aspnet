// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.TypeSystemDirectiveTestData
{
    using GraphQL.AspNet.Attributes;

    public enum MarkedEnumValue
    {
        Value1,

        [ApplyDirective(typeof(EnumValueMarkerDirective))]
        Value2,
    }
}