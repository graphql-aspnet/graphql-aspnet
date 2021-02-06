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
    public enum EnumWithDuplicateValuesFromComposite
    {
        Value1 = 1,
        Value2 = 2,
        Value3 = 3,

        // same as Value3 numerically
        // 1 | 2 == 3
        Value4SameAs3 = Value1 | Value2,
    }
}