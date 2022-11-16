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

    [GraphType("FragmentDataItem")]
    public interface IFragmentDataItem
    {
        [GraphField]
        string Property1 { get; }

        [GraphField]
        string Property2 { get; }
    }
}