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

    [GraphType("ValidGraphName")]
    public enum EnumWithGraphName
    {
        Value1,
        Value2,
    }
}