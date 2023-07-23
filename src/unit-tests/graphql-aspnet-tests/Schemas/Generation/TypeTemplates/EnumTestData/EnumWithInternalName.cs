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

    [GraphType(InternalName = "InternalName_Enum_21")]
    public enum EnumWithInternalName
    {
        Value1,
        Value2,
    }
}