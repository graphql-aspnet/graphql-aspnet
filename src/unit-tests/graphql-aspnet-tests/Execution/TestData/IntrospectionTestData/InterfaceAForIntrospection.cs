// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData
{
    using System.ComponentModel;

    public interface InterfaceAForIntrospection
    {
        [Description("Description of field A")]
        string FieldA { get; set; }
    }
}