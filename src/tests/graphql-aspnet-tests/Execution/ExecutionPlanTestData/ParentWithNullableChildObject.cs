// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    public class ParentWithNullableChildObject
    {
        public ParentWithNullableChildObject()
        {
            this.Child = new NullableChildObject()
            {
                Property2 = "child default value",
            };

            this.Property1 = "prop1 default value";
        }

        public string Property1 { get; set; }

        public NullableChildObject Child { get; set; }
    }
}