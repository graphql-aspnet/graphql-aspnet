// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Defaults.TypeMakers
{
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using NUnit.Framework;

    [TestFixture]
    public class GraphTypeMakerFactoryTests : GraphTypeMakerTestBase
    {
        [Test]
        public void DefaultFactory_NoSchema_YieldsNoMaker()
        {
            var factory = new DefaultGraphTypeMakerProvider();
            var instance = factory.CreateTypeMaker(null, TypeKind.OBJECT);
            Assert.IsNull(instance);
        }

        [Test]
        public void DefaultFactory_UnknownTypeKind_YieldsNoMaker()
        {
            var schema = new GraphSchema();
            var factory = new DefaultGraphTypeMakerProvider();
            var instance = factory.CreateTypeMaker(schema, TypeKind.LIST);
            Assert.IsNull(instance);
        }
    }
}