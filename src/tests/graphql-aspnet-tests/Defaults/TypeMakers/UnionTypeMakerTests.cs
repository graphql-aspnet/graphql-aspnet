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
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Defaults.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class UnionTypeMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void ActionTemplate_CreateUnionType_PropertyCheck()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();

            var action = TemplateHelper.CreateActionMethodTemplate<UnionTestController>(nameof(UnionTestController.TwoTypeUnion));
            var union = new UnionGraphTypeMaker(schema).CreateGraphType(action.UnionProxy);

            Assert.IsNotNull(union);
            Assert.IsTrue(union is UnionGraphType);
            Assert.AreEqual("FragmentData", union.Name);
            Assert.IsNull(union.Description);

            Assert.AreEqual(2, union.PossibleGraphTypeNames.Count());
            Assert.AreEqual(2, union.PossibleConcreteTypes.Count());

            Assert.IsTrue(union.PossibleGraphTypeNames.Contains(nameof(UnionDataA)));
            Assert.IsTrue(union.PossibleGraphTypeNames.Contains(nameof(UnionDataB)));
            Assert.IsTrue(union.PossibleConcreteTypes.Contains(typeof(UnionDataA)));
            Assert.IsTrue(union.PossibleConcreteTypes.Contains(typeof(UnionDataB)));
        }

        [Test]
        public void UnionProxyWithDirectives_DirectivesAreApplied()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();

            var maker = new UnionGraphTypeMaker(schema);
            var unionType = maker.CreateGraphType(new UnionProxyWithDirective());

            Assert.IsNotNull(unionType);
            Assert.AreEqual(1, unionType.AppliedDirectives.Count);
            Assert.AreEqual(unionType, unionType.AppliedDirectives.Parent);

            var appliedDirective = unionType.AppliedDirectives[0];
            Assert.IsNotNull(appliedDirective);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 121, "union directive" }, appliedDirective.Arguments);
        }

        [Test]
        public void UnionProxyWithDirectives_InvalidDirectiveType_ThrowsException()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();
            var maker = new UnionGraphTypeMaker(schema);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var unionType = maker.CreateGraphType(new UnionProxyWithInvalidDirective());
            });
        }

        [Test]
        public void NullProxy_YieldsNullGraphType()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();
            var maker = new UnionGraphTypeMaker(schema);

            var unionType = maker.CreateGraphType(null);
            Assert.IsNull(unionType);
        }
    }
}