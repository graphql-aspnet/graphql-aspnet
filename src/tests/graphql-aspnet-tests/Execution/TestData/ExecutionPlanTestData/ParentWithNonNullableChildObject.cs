// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData
{
    using GraphQL.AspNet.Attributes;

    public class ParentWithNonNullableChildObject
    {
        public ParentWithNonNullableChildObject()
        {
            this.Child = new NullableChildObject();
        }

        public string Property1 { get; set; }

        [GraphField(TypeExpression = AspNet.Schemas.TypeSystem.TypeExpressions.IsNotNull)]
        public NullableChildObject Child { get; set; }
    }
}