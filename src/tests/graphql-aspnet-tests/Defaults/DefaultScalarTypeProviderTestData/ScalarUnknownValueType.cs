// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Defaults.DefaultScalarTypeProviderTestData
{
    using GraphQL.AspNet.Parsing2;

    public class ScalarUnknownValueType : ScalarTestBase
    {
        public ScalarUnknownValueType()
        {
            this.ValueType = ScalarValueType.Unknown;
        }
    }
}