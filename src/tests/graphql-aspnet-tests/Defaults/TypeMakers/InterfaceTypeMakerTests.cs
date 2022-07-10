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
    using GraphQL.AspNet.Tests.Defaults.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class InterfaceTypeMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void CreateGraphType_PropertyCheck()
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

        [Test]
        public void CreateGraphType_DirectivesAreApplied()
        {
            var result = this.MakeGraphType(typeof(InterfaceWithDirective), TypeKind.INTERFACE);
            var interfaceType = result.GraphType as IInterfaceGraphType;

            Assert.IsNotNull(interfaceType);
            Assert.AreEqual(1, interfaceType.AppliedDirectives.Count);
            Assert.AreEqual(interfaceType, interfaceType.AppliedDirectives.Parent);

            var appliedDirective = interfaceType.AppliedDirectives.FirstOrDefault();
            Assert.IsNotNull(appliedDirective);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 58, "interface arg" }, appliedDirective.ArgumentValues);
        }

        [Test]
        public void CreateGraphType_DeclaredInterfacesAreCaptured()
        {
            var result = this.MakeGraphType(typeof(ITestInterfaceForDeclaredInterfaces), TypeKind.INTERFACE);
            var interfaceType = result.GraphType as IInterfaceGraphType;

            Assert.IsNotNull(interfaceType);

            // __typename and Field3
            // only those declared on the interface, not those inherited
            Assert.AreEqual(2, interfaceType.Fields.Count());

            // should reference the two additional interfaces
            // ITestInterface1 ITestInterface2
            Assert.AreEqual(2, interfaceType.InterfaceNames.Count());
            Assert.IsTrue(interfaceType.InterfaceNames.Any(x => x == "ITestInterface1"));
            Assert.IsTrue(interfaceType.InterfaceNames.Any(x => x == "ITestInterface2"));
        }
    }
}