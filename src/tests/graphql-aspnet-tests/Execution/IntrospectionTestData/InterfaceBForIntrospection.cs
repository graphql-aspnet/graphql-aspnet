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

    public interface InterfaceBForIntrospection : InterfaceAForIntrospection
    {
        [Description("Description of field B")]
        string FieldB { get; set; }
    }
}