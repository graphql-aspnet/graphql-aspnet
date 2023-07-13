// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Internal.Templating.EnumTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;

    public enum EnumWithDirectiveOnOption
    {
        Value1,

        [ApplyDirective(typeof(DirectiveWithArgs), 89, "enum option arg")]
        Value2,
    }
}