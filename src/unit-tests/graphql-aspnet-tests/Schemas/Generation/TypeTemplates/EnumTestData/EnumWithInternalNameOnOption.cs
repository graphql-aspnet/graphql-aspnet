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

    public enum EnumWithInternalNameOnOption
    {
        [GraphEnumValue(InternalName = "Value1_InternalName")]
        Value1,
    }
}