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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class ComplexInputObjectController : GraphController
    {
        [MutationRoot(typeof(bool))]
        public IGraphActionResult AddObject(ComplexInputObjectWithNoRequiredFields objectA)
        {
            return this.Ok(true);
        }
    }
}