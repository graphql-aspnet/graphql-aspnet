// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating
{
    using System.Linq;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Internal.Templating.ActionTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GraphActionTemplateTests
    {
        private ControllerActionGraphFieldTemplate CreateActionTemplate<TControllerType>(string actionName)
            where TControllerType : GraphController
        {
            var mockController = new Mock<IGraphControllerTemplate>();
            mockController.Setup(x => x.InternalFullName).Returns(typeof(TControllerType).Name);
            mockController.Setup(x => x.Route).Returns(new GraphFieldPath("path0"));
            mockController.Setup(x => x.Name).Returns("path0");
            mockController.Setup(x => x.ObjectType).Returns(typeof(TControllerType));

            var methodInfo = typeof(TControllerType).GetMethod(actionName);
            var action = new ControllerActionGraphFieldTemplate(mockController.Object, methodInfo);
            action.Parse();
            action.ValidateOrThrow();

            return action;
        }

        [Test]
        public void ActionTemplate_Parse_BasicPropertySets()
        {
            var methodInfo = typeof(OneMethodController).GetMethod(nameof(OneMethodController.MethodWithBasicAttribtributes));
            var action = this.CreateActionTemplate<OneMethodController>(nameof(OneMethodController.MethodWithBasicAttribtributes));
            action.ValidateOrThrow();

            Assert.AreEqual("MethodDescription", action.Description);
            Assert.AreEqual(typeof(OneMethodController), action.SourceObjectType);
            Assert.AreEqual(typeof(OneMethodController), action.Parent.ObjectType);
            Assert.AreEqual(GraphCollection.Query, action.Route.RootCollection);
            Assert.AreEqual("[query]/path0/path1", action.Route.Path);
            Assert.AreEqual($"{nameof(OneMethodController)}.{nameof(OneMethodController.MethodWithBasicAttribtributes)}", action.InternalFullName);
            Assert.AreEqual(methodInfo.ReflectedType, ((IGraphMethod)action).Parent.ObjectType);
            Assert.AreEqual("path0", ((IGraphMethod)action).Parent.Name);
            Assert.AreEqual(methodInfo, action.Method);
            Assert.AreEqual(0, action.Arguments.Count);
            Assert.IsFalse(action.Route.IsTopLevelField);
            Assert.IsFalse(action.IsAsyncField);
            Assert.IsFalse(action.IsDeprecated);
        }

        [Test]
        public void ActionTemplate_Parse_MethodMarkedAsOperationIsAssignedARootPath()
        {
            var action = this.CreateActionTemplate<ContainerController>(nameof(ContainerController.RootMethod));

            Assert.AreEqual(GraphCollection.Query, action.Route.RootCollection);
            Assert.AreEqual(0, action.Arguments.Count);
            Assert.IsFalse(action.IsAsyncField);
            Assert.AreEqual("[query]/path22", action.Route.Path);
        }

        [Test]
        public void ActionTemplate_Parse_WithValidDeclaredUnion_ParsesCorrectly()
        {
            var action = this.CreateActionTemplate<UnionTestController>(nameof(UnionTestController.TwoTypeUnion));

            Assert.AreEqual(GraphCollection.Query, action.Route.RootCollection);
            Assert.IsNotNull(action.UnionProxy);
            Assert.AreEqual(2, action.UnionProxy.Types.Count);
            Assert.AreEqual(action.ObjectType, typeof(object));
            Assert.IsTrue(action.TypeExpression.IsListOfItems);
            Assert.IsTrue(action.UnionProxy.Types.Contains(typeof(UnionDataA)));
            Assert.IsTrue(action.UnionProxy.Types.Contains(typeof(UnionDataB)));
            Assert.IsNull(action.UnionProxy.Description);
            Assert.AreEqual("FragmentData", action.UnionProxy.Name);
        }

        [Test]
        public void ActionTemplate_Parse_WithValidDeclaredUnionViaProxy_UsesProxy()
        {
            var action = this.CreateActionTemplate<UnionTestController>(nameof(UnionTestController.UnionViaProxy));

            Assert.AreEqual(GraphCollection.Query, action.Route.RootCollection);
            Assert.IsNotNull(action.UnionProxy);
            Assert.AreEqual(typeof(UnionTestProxy), action.UnionProxy.GetType());
        }

        [TestCase(nameof(UnionTestController.NoTypesUnion))]
        [TestCase(nameof(UnionTestController.ScalarInUnion))]
        [TestCase(nameof(UnionTestController.InterfaceDefinedAsUnionMember))]
        [TestCase(nameof(UnionTestController.UnionProxyAsUnionMember))]
        public void ActionTemplate_Parse_ThrowsException(string methodName)
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateActionTemplate<UnionTestController>(methodName);
            });
        }

        [Test]
        public void ActionTemplate_NegativeComplexityValue_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<ComplexityValueCheckController>(nameof(ComplexityValueCheckController.ReturnsAnInt));
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfActionResult_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateActionTemplate<ActionResultReturnTypeController>(nameof(ActionResultReturnTypeController.ActionResultMethod));
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfActionResult_WithDeclaredDataType_RendersCorrectly()
        {
            var action = this.CreateActionTemplate<ActionResultReturnTypeController>(nameof(ActionResultReturnTypeController.ActionResultMethodWithDeclaredReturnType));

            Assert.AreEqual(typeof(TwoPropertyObject), action.ObjectType);
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfActionResult_WithDeclaredListDataType_RendersOptionsCorrectly()
        {
            var action = this.CreateActionTemplate<ActionResultReturnTypeController>(nameof(ActionResultReturnTypeController.ActionResultMethodWithListReturnType));

            Assert.AreEqual(typeof(TwoPropertyObject), action.ObjectType);
            Assert.IsTrue(action.TypeExpression.IsListOfItems);
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfActionResult_WithDeclaredListDataType_AndSpecificOptions_RendersOptionsCorrectly()
        {
            var action = this.CreateActionTemplate<ActionResultReturnTypeController>(nameof(ActionResultReturnTypeController.ActionResultMethodWithListReturnTypeAndOptions));
            action.ValidateOrThrow();

            Assert.AreEqual(typeof(TwoPropertyObject), action.ObjectType);
            CollectionAssert.AreEqual(
                new[] { MetaGraphTypes.IsNotNull, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull },
                action.TypeExpression.Wrappers);
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfActionResult_WithDeclaredDataType_ThatHasCustomName_UsesCustomNameInTypeExpression()
        {
            var action = this.CreateActionTemplate<ActionResultReturnTypeController>(nameof(ActionResultReturnTypeController.ActionResultWithCustomNamedReturnedItem));
            action.ValidateOrThrow();

            Assert.AreEqual(typeof(CustomNamedItem), action.ObjectType);
            Assert.AreEqual("AnAwesomeName", action.TypeExpression.TypeName);
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfActionResult_WithDeclaredDataType_DiffersFromMethodReturn_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<ActionResultReturnTypeController>(
                    nameof(ActionResultReturnTypeController
                        .ActionResultMethodWithDeclaredReturnTypeAndMethodReturnType));
                action.ValidateOrThrow();
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfInterface_WithPossibleTypesAttribute_RetrieveDependentTypes_ReturnsAllTypes()
        {
            var action = this.CreateActionTemplate<InterfaceReturnTypeController>(nameof(InterfaceReturnTypeController.RetrieveData));

            var types = action.RetrieveRequiredTypes();
            Assert.IsNotNull(types);
            Assert.AreEqual(3, types.Count());

            Assert.IsTrue(types.Any(x => x.Type == typeof(TestItemA)));
            Assert.IsTrue(types.Any(x => x.Type == typeof(TestItemB)));
            Assert.IsTrue(types.Any(x => x.Type == typeof(ITestItem)));
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfInterface_WithoutPossibleTypesAttribute_DoesntFail_ReturnsOnlyInterface()
        {
            var action = this.CreateActionTemplate<InterfaceReturnTypeController>(nameof(InterfaceReturnTypeController.RetrieveDataNoAttributeDeclared));

            var types = action.RetrieveRequiredTypes();
            Assert.IsNotNull(types);
            Assert.AreEqual(1, types.Count());

            Assert.IsTrue(types.Any(x => x.Type == typeof(ITestItem)));
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfInterface_WithPossibleTypesAttribute_ButTypeDoesntImplementInterface_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<InterfaceReturnTypeController>(nameof(InterfaceReturnTypeController.RetrieveDataInvalidPossibleType));
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfInterface_WithPossibleTypesAttribute_ButTypeIsAnInterface_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<InterfaceReturnTypeController>(nameof(InterfaceReturnTypeController.RetrieveDataPossibleTypeIsInterface));
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfInterface_WithPossibleTypesAttribute_ButTypeIsAScalar_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<InterfaceReturnTypeController>(nameof(InterfaceReturnTypeController.RetrieveDataPossibleTypeIsScalar));
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfInterface_WithPossibleTypesAttribute_ButTypeIsAStruct_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<InterfaceReturnTypeController>(nameof(InterfaceReturnTypeController.RetrieveDataPossibleTypeIsAStruct));
            });
        }

        [Test]
        public void ActionTemplate_ArrayOnInputParameter_RendersFine()
        {
            var action = this.CreateActionTemplate<ArrayInputMethodController>(nameof(ArrayInputMethodController.AddData));

            var types = action.RetrieveRequiredTypes();
            Assert.IsNotNull(types);
            Assert.AreEqual(1, types.Count());

            Assert.IsTrue(types.Any(x => x.Type == typeof(string)));

            Assert.AreEqual(1, action.Arguments.Count);
            Assert.AreEqual(typeof(TwoPropertyObject[]), action.Arguments[0].DeclaredArgumentType);
            Assert.AreEqual(typeof(TwoPropertyObject), action.Arguments[0].ObjectType);
        }
    }
}