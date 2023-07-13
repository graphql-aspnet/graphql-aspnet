// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.TypeSystemDirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    [ApplyDirective(typeof(UnionMarkerDirective))]
    public class MarkedUnion : GraphUnionProxy
    {
        public MarkedUnion()
            : base("MyUnion", typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2))
        {
        }
    }
}