// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration.PlanGenerationTestData
{
    using GraphQL.AspNet.Attributes;

    public class FragmentDataA : IFragmentDataItem
    {
        private string _property1;
        private string _property2;

        [GraphField]
        public string Property1
        {
            get => "fragmentA_prop1_" + _property1;
            set => _property1 = value;
        }

        [GraphField]
        public string Property2
        {
            get => "fragmentA_prop2_" + _property2;
            set => _property2 = value;
        }
    }
}