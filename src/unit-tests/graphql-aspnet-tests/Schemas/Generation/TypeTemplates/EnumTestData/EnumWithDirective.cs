// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.EnumTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData;

    [ApplyDirective(typeof(DirectiveWithArgs), 5, "bob")]
    public enum EnumWithDirective
    {
        Value1,
        Value2,
    }
}