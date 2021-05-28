namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;

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
    }
}
