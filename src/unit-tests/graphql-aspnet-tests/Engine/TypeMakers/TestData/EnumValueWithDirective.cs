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

    public enum EnumValueWithDirective
    {
        Value1,

        [ApplyDirective(typeof(DirectiveWithArgs), 33, "enum value arg")]
        Value2,
    }
}