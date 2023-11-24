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
    using System.ComponentModel;

    public enum EnumWithDescriptionOnValues
    {
        Value1,

        [Description("Value2 Description")]
        Value2,

        Value3,

        [Description("Value4 Description")]
        Value4,
    }
}