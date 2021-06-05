﻿// *************************************************************
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
    }
}