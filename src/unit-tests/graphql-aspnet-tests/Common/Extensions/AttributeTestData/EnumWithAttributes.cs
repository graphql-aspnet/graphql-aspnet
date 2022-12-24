// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.Extensions.AttributeTestData
{
    public enum EnumWithAttributes
    {
        [InheritsFromBaseTest]
        ValueWithOneAttribute,

        [InheritsFromBaseTest]
        [InheritsFromBaseTest]
        ValueWith2Attributes,

        [Other]
        ValueWithOtherAttribute,

        ValueWithNoAttributes,
    }
}