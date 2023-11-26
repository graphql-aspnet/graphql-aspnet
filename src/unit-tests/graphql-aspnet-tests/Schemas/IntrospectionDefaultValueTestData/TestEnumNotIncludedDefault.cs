// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.IntrospectionDefaultValueTestData
{
    using GraphQL.AspNet.Attributes;

    public enum TestEnumNotIncludedDefault
    {
        [GraphSkip]
        Value1,
        Value2,
        Value3,
    }
}