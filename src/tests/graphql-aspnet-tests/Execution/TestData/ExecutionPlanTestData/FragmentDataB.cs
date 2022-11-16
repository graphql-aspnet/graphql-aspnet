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

    public class FragmentDataB : IFragmentDataItem
    {
        private string _property2;
        private string _property1;

        [GraphField]
        public string Property1
        {
            get => "fragmentB_prop1_" + _property1;
            set => _property1 = value;
        }

        [GraphField]
        public string Property2
        {
            get => "fragmentB_prop2_" + _property2;
            set => _property2 = value;
        }
    }
}