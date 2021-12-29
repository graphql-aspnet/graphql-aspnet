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
    using System.Linq;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Default.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class InterfaceTypeMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void Parse_FromInterface_CreateGraphType_PropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = TemplateHelper.CreateInterfaceTemplate<ISimpleInterface>();
            var typeMaker = new DefaultGraphTypeMakerProvider();

            var graphType = typeMaker.CreateTypeMaker(server.Schema, TypeKind.INTERFACE)
                .CreateGraphType(typeof(ISimpleInterface)).GraphType as IInterfaceGraphType;
            Assert.IsNotNull(graphType);

            Assert.AreEqual(template.Name, graphType.Name);
            Assert.AreEqual(TypeKind.INTERFACE, graphType.Kind);

            // Property1, Property2, __typename
            Assert.AreEqual(3, graphType.Fields.Count());
        }
    }
}