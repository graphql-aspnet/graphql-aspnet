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
    public class ScalarInvalidGraphName : ScalarTestBase
    {
        public ScalarInvalidGraphName()
        {
            this.Name = "__InvalidName";
        }
    }
}