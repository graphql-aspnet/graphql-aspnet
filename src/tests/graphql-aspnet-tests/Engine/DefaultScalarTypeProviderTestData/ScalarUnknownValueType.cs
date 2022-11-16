// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.DefaultScalarTypeProviderTestData
{
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;

    public class ScalarUnknownValueType : ScalarTestBase
    {
        public ScalarUnknownValueType()
        {
            this.ValueType = ScalarValueType.Unknown;
        }
    }
}