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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ParentWithNonNullableChildObject
    {
        public string Property1 { get; set; }

        [GraphField(TypeExpression = TypeExpressions.IsNotNull)]
        public NullableChildObject Child { get; set; }
    }
}