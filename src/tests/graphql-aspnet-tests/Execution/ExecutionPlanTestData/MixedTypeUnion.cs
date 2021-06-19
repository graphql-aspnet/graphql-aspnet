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
    using System;
    using GraphQL.AspNet.Schemas.TypeSystem;

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