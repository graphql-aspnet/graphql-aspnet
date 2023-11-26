// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates
{
    using System.Linq;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ActionTestData;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class ActionMethodTemplateTests
    {
        private ControllerActionGraphFieldTemplate CreateActionTemplate<TControllerType>(string actionName)
            where TControllerType : GraphController
        {
            var mockController = Substitute.For<IGraphControllerTemplate>();
            mockController.InternalName.Returns(typeof(TControllerType).Name);
            mockController.ItemPath.Returns(new ItemPath("path0"));
            mockController.Name.Returns("path0");
            mockController.ObjectType.Returns(typeof(TControllerType));

            var methodInfo = typeof(TControllerType).GetMethod(actionName);
            var action = new ControllerActionGraphFieldTemplate(mockController, methodInfo);
            action.Parse();

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
            Assert.AreEqual(ItemPathRoots.Query, action.ItemPath.Root);
            Assert.AreEqual("[query]/path0/path1", action.ItemPath.Path);
            Assert.AreEqual($"{nameof(OneMethodController)}.{nameof(OneMethodController.MethodWithBasicAttribtributes)}", action.InternalName);
            Assert.AreEqual(methodInfo.ReflectedType, action.Parent.ObjectType);
            Assert.AreEqual("path0", action.Parent.Name);
            Assert.AreEqual(methodInfo, action.Method);
            Assert.AreEqual(0, action.Arguments.Count);
            Assert.IsFalse(action.ItemPath.IsTopLevelField);
            Assert.IsFalse(action.IsAsyncField);
        }

        [Test]
        public void ActionTemplate_Parse_MethodMarkedAsOperationIsAssignedARootPath()
        {
            var action = this.CreateActionTemplate<ContainerController>(nameof(ContainerController.RootMethod));
            action.ValidateOrThrow();

            Assert.AreEqual(ItemPathRoots.Query, action.ItemPath.Root);
            Assert.AreEqual(0, action.Arguments.Count);
            Assert.IsFalse(action.IsAsyncField);
            Assert.AreEqual("[query]/path22", action.ItemPath.Path);
        }

        [Test]
        public void ActionTemplate_Parse_WithValidDeclaredUnion_ParsesCorrectly()
        {
            var action = this.CreateActionTemplate<UnionTestController>(nameof(UnionTestController.TwoTypeUnion));
            action.ValidateOrThrow();

            Assert.AreEqual(ItemPathRoots.Query, action.ItemPath.Root);
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
            action.ValidateOrThrow();

            Assert.AreEqual(ItemPathRoots.Query, action.ItemPath.Root);
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
                var template = this.CreateActionTemplate<UnionTestController>(methodName);
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void ActionTemplate_NegativeComplexityValue_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<ComplexityValueCheckController>(nameof(ComplexityValueCheckController.ReturnsAnInt));
                action.ValidateOrThrow();
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfActionResult_WithoutDeclaredReturnType_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<ActionResultReturnTypeController>(nameof(ActionResultReturnTypeController.ActionResultMethod));
                action.ValidateOrThrow();
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfActionResult_WithDeclaredDataType_RendersCorrectly()
        {
            var action = this.CreateActionTemplate<ActionResultReturnTypeController>(nameof(ActionResultReturnTypeController.ActionResultMethodWithDeclaredReturnType));
            action.ValidateOrThrow();

            Assert.AreEqual(typeof(TwoPropertyObject), action.ObjectType);
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfActionResult_WithDeclaredListDataType_RendersObjectTypeCorrectly()
        {
            var action = this.CreateActionTemplate<ActionResultReturnTypeController>(nameof(ActionResultReturnTypeController.ActionResultMethodWithListReturnType));
            action.ValidateOrThrow();

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
            Assert.AreEqual("Type", action.TypeExpression.TypeName);
        }

        [Test]
        public void ActionTemplate_WithDeclaredDataType_ThatDiffersFromMethodReturn_ThrowsException()
        {
            var ex = Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<ActionResultReturnTypeController>(
                    nameof(ActionResultReturnTypeController.MethodWithDeclaredReturnTypeAndMethodReturnType));
                action.ValidateOrThrow();
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfInterface_WithPossibleTypesAttribute_RetrieveDependentTypes_ReturnsAllTypes()
        {
            var action = this.CreateActionTemplate<InterfaceReturnTypeController>(nameof(InterfaceReturnTypeController.RetrieveData));
            action.ValidateOrThrow();

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
            action.ValidateOrThrow();

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
                action.ValidateOrThrow();
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfInterface_WithPossibleTypesAttribute_ButTypeIsAnInterface_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<InterfaceReturnTypeController>(nameof(InterfaceReturnTypeController.RetrieveDataPossibleTypeIsInterface));
                action.ValidateOrThrow();
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfInterface_WithPossibleTypesAttribute_ButTypeIsAScalar_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<InterfaceReturnTypeController>(nameof(InterfaceReturnTypeController.RetrieveDataPossibleTypeIsScalar));
                action.ValidateOrThrow();
            });
        }

        [Test]
        public void ActionTemplate_ReturnTypeOfInterface_WithPossibleTypesAttribute_ButTypeIsAStruct_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var action = this.CreateActionTemplate<InterfaceReturnTypeController>(nameof(InterfaceReturnTypeController.RetrieveDataPossibleTypeIsAStruct));
                action.ValidateOrThrow();
            });
        }

        [Test]
        public void ActionTemplate_ArrayOnInputParameter_RendersFine()
        {
            var action = this.CreateActionTemplate<ArrayInputMethodController>(nameof(ArrayInputMethodController.AddData));
            action.ValidateOrThrow();

            var types = action.RetrieveRequiredTypes();
            Assert.IsNotNull(types);

            Assert.IsTrue(types.Any(x => x.Type == typeof(string)));

            Assert.AreEqual(1, action.Arguments.Count);
            Assert.AreEqual(typeof(TwoPropertyObject[]), action.Arguments[0].DeclaredArgumentType);
            Assert.AreEqual(typeof(TwoPropertyObject), action.Arguments[0].ObjectType);
        }

        [Test]
        public void Parse_AssignedDirective_IsTemplatized()
        {
            var action = this.CreateActionTemplate<ActionMethodWithDirectiveController>(nameof(ActionMethodWithDirectiveController.Execute));
            action.ValidateOrThrow();

            Assert.AreEqual(1, action.AppliedDirectives.Count());

            var appliedDirective = action.AppliedDirectives.First();
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            Assert.AreEqual(new object[] { 202, "controller action arg" }, appliedDirective.Arguments);
        }

        [Test]
        public void Parse_InternalName_IsAssignedCorrectly()
        {
            var action = this.CreateActionTemplate<ActionMethodWithInternalNameController>(nameof(ActionMethodWithInternalNameController.Execute));
            action.ValidateOrThrow();

            Assert.AreEqual("Internal_Action_Name_37", action.InternalName);
        }
    }
}