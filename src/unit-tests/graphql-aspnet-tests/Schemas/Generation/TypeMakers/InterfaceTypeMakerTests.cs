// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers
{
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData;
    using NUnit.Framework;

    [TestFixture]
    public class InterfaceTypeMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void CreateGraphType_PropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateInterfaceTemplate<ISimpleInterface>();

            var factory = server.CreateMakerFactory();

            var typeMaker = new InterfaceGraphTypeMaker(server.Schema.Configuration, factory.CreateFieldMaker());

            var graphType = typeMaker.CreateGraphType(GraphQLTemplateHelper.CreateGraphTypeTemplate(typeof(ISimpleInterface), TypeKind.INTERFACE)).GraphType as IInterfaceGraphType;
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

        [Test]
        public void CreateGraphType_WhenMethodOnBaseInterfaceIsNotExplicitlyDeclared_WhenExplicitDeclarationIsRequired_IsNotIncluded()
        {
            var result = this.MakeGraphType(
                typeof(InterfaceThatInheritsUndeclaredMethodField),
                TypeKind.INTERFACE,
                TemplateDeclarationRequirements.Method);

            var objectType = result.GraphType as IInterfaceGraphType;

            // inherited, undeclared method field should not be counted
            Assert.IsNotNull(objectType);

            // property field + __typename
            Assert.AreEqual(2, objectType.Fields.Count);
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, nameof(InterfaceThatInheritsUndeclaredMethodField.PropFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, Constants.ReservedNames.TYPENAME_FIELD)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenMethodOnBaseInterfaceIsNotExplicitlyDeclared_WhenExplicitDeclarationIsNotRequired_IsNotIncluded()
        {
            var result = this.MakeGraphType(
                typeof(InterfaceThatInheritsUndeclaredMethodField),
                TypeKind.INTERFACE,
                TemplateDeclarationRequirements.None);

            var objectType = result.GraphType as IInterfaceGraphType;

            // inherited, undeclared method field should be counted
            Assert.IsNotNull(objectType);

            // property field + base field + __typename
            Assert.AreEqual(3, objectType.Fields.Count);
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, nameof(InterfaceThatInheritsUndeclaredMethodField.MethodFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, nameof(InterfaceThatInheritsUndeclaredMethodField.PropFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, Constants.ReservedNames.TYPENAME_FIELD)));
        }

        [Test]
        public void CreateGraphType_WhenMethodOnBaseInterfaceIsExplicitlyDeclared_IsNotIncluded()
        {
            var result = this.MakeGraphType(
                typeof(InterfaceThatInheritsDeclaredMethodField),
                TypeKind.INTERFACE,
                TemplateDeclarationRequirements.None);

            var objectType = result.GraphType as IInterfaceGraphType;

            // inherited and declared method field should not be counted
            Assert.IsNotNull(objectType);

            // property field + base field + __typename
            Assert.AreEqual(3, objectType.Fields.Count);
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, nameof(InterfaceThatInheritsDeclaredMethodField.MethodFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, nameof(InterfaceThatInheritsDeclaredMethodField.PropFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, Constants.ReservedNames.TYPENAME_FIELD)));
        }
    }
}