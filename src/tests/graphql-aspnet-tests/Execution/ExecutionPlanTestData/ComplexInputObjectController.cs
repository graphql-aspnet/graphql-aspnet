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
        [MutationRoot(typeof(ComplexInputObjectWithNoRequiredFields))]
        public IGraphActionResult AddObject(ComplexInputObjectWithNoRequiredFields objectA)
        {
            return this.Ok(objectA);
        }

        [MutationRoot(typeof(ParentWithNullableChildObject))]
        public IGraphActionResult ObjectWithNullChild(ParentWithNullableChildObject parentObj)
        {
            return this.Ok(parentObj);
        }

        [MutationRoot(typeof(ParentWithNonNullableChildObject))]
        public IGraphActionResult ObjectWithNonNullChild(ParentWithNonNullableChildObject parentObj)
        {
            return this.Ok(parentObj);
        }
    }
}