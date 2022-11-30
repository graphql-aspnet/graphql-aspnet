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
    public class ScalarNoSourceResolver : ScalarTestBase
    {
        public ScalarNoSourceResolver()
        {
            this.SourceResolver = null;
        }
    }
}