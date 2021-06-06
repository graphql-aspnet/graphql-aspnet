namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Internal;

    public class MixedTypeUnionInvalidReturn : GraphUnionProxy
    {
        public static int TotalCallCount = 0;

        public MixedTypeUnionInvalidReturn()
            : base(typeof(MixedReturnTypeA), typeof(MixedReturnTypeB))
        {
        }

        public override Type ResolveType(Type runtimeObjectType)
        {
            // return a type that isn't part of the union
            TotalCallCount += 1;
            return typeof(ObjectWithThrowMethod);
        }
    }
}
