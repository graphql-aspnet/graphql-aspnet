// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Defaults.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;

    public enum EnumWithUndeclaredValues
    {
        [GraphEnumValue]
        DeclaredValue1,

        [GraphEnumValue("VALUE_AWESOME")]
        DeclaredValue2,

        UndeclaredValue1,
    }
}