// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.GraphSkipSequencingTestData
{
    using GraphQL.AspNet.Attributes;

    public class ClassWithRenamedField
    {
        [GraphSkip]
        public int RootNamedField { get; set; }

        [GraphField("RootNamedField")]
        public string ReNamedField { get; set; }
    }
}
