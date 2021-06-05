namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Internal;

    public class MixedTypeUnion : GraphUnionProxy
    {
        public static int TotalCallCount = 0;

        public MixedTypeUnion()
            : base("MixedTypeUnion", typeof(MixedReturnTypeA), typeof(MixedReturnTypeB))
        {
        }

        public override Type ResolveType(Type runtimeObjectType)
        {
            TotalCallCount += 1;
            if (runtimeObjectType == typeof(MixedReturnTypeC))
                return typeof(MixedReturnTypeA);

            return runtimeObjectType;
        }
    }
}
