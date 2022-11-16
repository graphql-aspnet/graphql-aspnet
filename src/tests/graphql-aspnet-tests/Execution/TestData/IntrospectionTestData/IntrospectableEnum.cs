// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.IntrospectionTestData
{
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;

    public enum IntrospectableEnum
    {
        [Description("This is value 1")]
        Value1,

        [Description("This is value 2")]
        [Deprecated("To be removed soon")]
        Value2,
    }
}