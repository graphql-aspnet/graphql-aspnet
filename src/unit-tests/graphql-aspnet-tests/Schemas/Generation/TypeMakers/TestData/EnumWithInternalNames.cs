// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;

    [GraphType(InternalName = "EnumInternalName")]
    public enum EnumWithInternalNames
    {
        [GraphEnumValue(InternalName = "Value1InternalName")]
        Value1,
    }
}