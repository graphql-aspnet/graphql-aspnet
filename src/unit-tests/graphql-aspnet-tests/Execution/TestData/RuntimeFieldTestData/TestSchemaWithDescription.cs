// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.RuntimeFieldTestData
{
    using GraphQL.AspNet.Schemas;

    public class TestSchemaWithDescription : GraphSchema
    {
        public TestSchemaWithDescription()
        {
            this.Description = "My Test Description";
        }
    }
}