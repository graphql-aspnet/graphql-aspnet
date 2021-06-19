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
    using GraphQL.AspNet.Interfaces.Controllers;

    public class MixedReturnTypeController : GraphIdController
    {
        [QueryRoot]
        public MixedReturnTypeA CreateReturnObject()
        {
            return new MixedReturnTypeB()
            {
                Field1 = "FieldValue1",
                Field2 = "FieldValue2",
            };
        }

        [QueryRoot("createIndeterminateReturn", typeof(MixedTypeUnion))]
        public IGraphActionResult CreateIndeterminatateReturnObject()
        {
            var result = new MixedReturnTypeC()
            {
                Field1 = "FieldValue1",
                Field2 = "FieldValue2",
                Field3 = "FieldValue3",
            };

            return this.Ok(result);
        }

        [QueryRoot("createWrongIndeterminateReturn", typeof(MixedTypeUnionInvalidReturn))]
        public IGraphActionResult CreateWrongIndeterminatateReturnObject()
        {
            var result = new MixedReturnTypeC()
            {
                Field1 = "FieldValue1",
                Field2 = "FieldValue2",
                Field3 = "FieldValue3",
            };

            return this.Ok(result);
        }

        [QueryRoot("createNullIndeterminateReturn", typeof(MixedTypeUnionNullReturn))]
        public IGraphActionResult CreateNullIndeterminatateReturnObject()
        {
            var result = new MixedReturnTypeC()
            {
                Field1 = "FieldValue1",
                Field2 = "FieldValue2",
                Field3 = "FieldValue3",
            };

            return this.Ok(result);
        }

        [QueryRoot("createSourceIndeterminateReturn", typeof(MixedTypeUnionSourceReturn))]
        public IGraphActionResult CreateSourceIndeterminatateReturnObject()
        {
            var result = new MixedReturnTypeC()
            {
                Field1 = "FieldValue1",
                Field2 = "FieldValue2",
                Field3 = "FieldValue3",
            };

            return this.Ok(result);
        }
    }
}