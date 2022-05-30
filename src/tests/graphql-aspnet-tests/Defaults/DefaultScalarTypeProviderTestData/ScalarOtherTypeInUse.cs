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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ScalarOtherTypeInUse : ScalarTestBase
    {
        public ScalarOtherTypeInUse()
        {
            this.OtherKnownTypes = new TypeCollection(typeof(int));
        }
    }
}