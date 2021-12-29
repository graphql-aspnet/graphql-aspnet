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
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Default.TypeMakers.TestData;
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
            var union = new UnionGraphTypeMaker(schema).CreateGraphType(action.UnionProxy, TypeKind.OBJECT);

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
    }
}