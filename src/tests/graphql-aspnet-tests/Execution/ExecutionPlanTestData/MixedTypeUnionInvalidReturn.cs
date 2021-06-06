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