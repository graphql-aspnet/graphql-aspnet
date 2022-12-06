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
    using GraphQL.AspNet.Interfaces.Controllers;

    public class TypeExpressionMismatchController : GraphIdController
    {
        [QueryRoot(typeof(int), TypeExpression = "[Type]")]
        public IGraphActionResult RetrieveData()
        {
            return this.Ok(15);
        }

        [QueryRoot(typeof(int))]
        public IGraphActionResult RetrieveDataViaArg([FromGraphQL(TypeExpression = "[Type]")] int arg1)
        {
            return this.Ok(15);
        }
    }
}