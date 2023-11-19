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
    using GraphQL.AspNet.Attributes;

    [GraphType("Type_With_Custom_Name")]
    public class TypeWithCustomName
    {
        public int Field1 { get; set; }

        [GraphField("FieldTwo")]
        public string Field2 { get; set; }
    }
}