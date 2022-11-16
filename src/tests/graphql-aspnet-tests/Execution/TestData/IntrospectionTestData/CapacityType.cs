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

    public enum CapacityType
    {
        [Description("A small room")]
        Small,

        Medium,

        [Deprecated("Room too big")]
        Large,
    }
}