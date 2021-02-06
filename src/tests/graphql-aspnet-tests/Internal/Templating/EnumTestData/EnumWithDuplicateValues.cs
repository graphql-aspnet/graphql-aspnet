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
    public enum EnumWithDuplicateValues
    {
        Value0 = -5,
        Value1 = 1,
        Value2SameAsOne = 1,
        Value3,
    }
}