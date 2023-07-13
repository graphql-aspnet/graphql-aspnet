// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.GraphTypeNameTestData
{
    using GraphQL.AspNet.Attributes;

    [GraphType("GreatEnum")]
    public enum EnumNameTestWithTypeName
    {
        Value1,
        Value2,
    }
}