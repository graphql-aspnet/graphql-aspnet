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
    public enum EnumFromSByte : sbyte
    {
        Value1 = (sbyte)1,
        Value2 = (sbyte)2,
        Value3 = (sbyte)3,

        Value4 = (sbyte)-4,
        Value5 = (sbyte)-5,
        Value6 = (sbyte)-6,
    }
}