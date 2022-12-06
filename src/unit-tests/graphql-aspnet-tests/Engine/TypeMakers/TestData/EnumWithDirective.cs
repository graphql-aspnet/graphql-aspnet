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
    using GraphQL.AspNet.Attributes;

    [ApplyDirective(typeof(DirectiveWithArgs), 23, "enum arg")]
    public enum EnumWithDirective
    {
        Value1,
        Value2,
    }
}