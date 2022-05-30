// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTestData
{
    using GraphQL.AspNet.Attributes;

    [ApplyDirective(typeof(EnumMarkerDirective))]
    public enum MarkedEnum
    {
        Value1,
        Value2,
    }
}