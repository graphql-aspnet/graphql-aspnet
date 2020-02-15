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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Internal.Templating.ExtensionMethodTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GraphTypeExtensionMethodTemplateTests
    {
        private GraphTypeExtensionFieldTemplate CreateExtensionTemplate<TControllerType>(string actionName)
            where TControllerType : GraphController
        {
            var mockController = new Mock<IGraphControllerTemplate>();
            mockController.Setup(x => x.InternalFullName).Returns(typeof(TControllerType).Name);
            mockController.Setup(x => x.Route).Returns(new GraphFieldPath("path0"));
            mockController.Setup(x => x.Name).Returns("path0");
            mockController.Setup(x => x.ObjectType).Returns(typeof(TControllerType));

            var methodInfo = typeof(TControllerType).GetMethod(actionName);
            var template = new GraphTypeExtensionFieldTemplate(mockController.Object, methodInfo);
            template.Parse();
            template.ValidateOrThrow();

            return template;
        }

        [Test]
        public void ValidSimpleExtension_PropertyCheck()
        {
            var methodInfo = typeof(SimpleExtensionMethodController).GetMethod(nameof(SimpleExtensionMethodController.TestExtensionMethod));
            var template = this.CreateExtensionTemplate<SimpleExtensionMethodController>(nameof(SimpleExtensionMethodController.TestExtensionMethod));

            Assert.AreEqual("ExtensionDescription", template.Description);
            Assert.AreEqual(GraphCollection.Types, template.Route.RootCollection);
            Assert.AreEqual(typeof(SimpleExtensionMethodController), template.Parent.ObjectType);
            Assert.AreEqual(typeof(TwoPropertyObject), template.SourceObjectType);
            Assert.AreEqual($"[type]/{nameof(TwoPropertyObject)}/Property3", template.Route.Path);
            Assert.AreEqual($"{nameof(SimpleExtensionMethodController)}.{nameof(SimpleExtensionMethodController.TestExtensionMethod)}", template.InternalFullName);
            Assert.AreEqual(methodInfo.ReflectedType, ((IGraphMethod)template).Parent.ObjectType);
            Assert.AreEqual(typeof(TwoPropertyObject), template.SourceObjectType);
            Assert.AreEqual("path0", ((IGraphMethod)template).Parent.Name);
            Assert.AreEqual(methodInfo, template.Method);
            Assert.AreEqual(typeof(TwoPropertyObjectV2), template.ObjectType);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual(0, template.TypeExpression.Wrappers.Length);
            Assert.IsFalse(template.Route.IsTopLevelField);
            Assert.IsFalse(template.IsAsyncField);
            Assert.IsFalse(template.IsDeprecated);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, template.Mode);

            // first arg should be declared for the source data
            Assert.IsTrue(template.Arguments[0].ArgumentModifiers.HasFlag(GraphArgumentModifiers.ParentFieldResult));
            Assert.IsTrue(template.Arguments[0].ArgumentModifiers.HasFlag(GraphArgumentModifiers.Internal));
        }

        [Test]
        public void ValidBatchExtension_PropertyCheck()
        {
            var methodInfo = typeof(SimpleExtensionMethodController).GetMethod(nameof(SimpleExtensionMethodController.BatchTestExtension));
            var template = this.CreateExtensionTemplate<SimpleExtensionMethodController>(nameof(SimpleExtensionMethodController.BatchTestExtension));

            Assert.AreEqual("ExtensionDescription", template.Description);
            Assert.AreEqual(methodInfo.ReflectedType, ((IGraphMethod)template).Parent.ObjectType);
            Assert.AreEqual(typeof(TwoPropertyObject), template.SourceObjectType);
            Assert.AreEqual("path0", ((IGraphMethod)template).Parent.Name);
            Assert.AreEqual(methodInfo, template.Method);
            Assert.AreEqual(typeof(int), template.ObjectType);
            CollectionAssert.AreEqual(new[] { MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull }, template.TypeExpression.Wrappers);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual(FieldResolutionMode.Batch, template.Mode);

            // first arg should be declared for the source data
            Assert.IsTrue(template.Arguments[0].ArgumentModifiers.HasFlag(GraphArgumentModifiers.ParentFieldResult));
            Assert.IsTrue(template.Arguments[0].ArgumentModifiers.HasFlag(GraphArgumentModifiers.Internal));
        }

        [Test]
        public void ValidBatchExtension_WithCustomReturnType_AndNoDeclaredTypeOnAttribute_PropertyCheck()
        {
            var methodInfo = typeof(SimpleExtensionMethodController).GetMethod(nameof(SimpleExtensionMethodController.CustomValidReturnType));
            var template = this.CreateExtensionTemplate<SimpleExtensionMethodController>(nameof(SimpleExtensionMethodController.CustomValidReturnType));

            Assert.AreEqual(methodInfo.ReflectedType, ((IGraphMethod)template).Parent.ObjectType);
            Assert.AreEqual(typeof(TwoPropertyObject), template.SourceObjectType);
            Assert.AreEqual(methodInfo, template.Method);
            CollectionAssert.AreEqual(new[] { MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull }, template.TypeExpression.Wrappers);
            Assert.AreEqual(typeof(int), template.ObjectType);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual(FieldResolutionMode.Batch, template.Mode);
        }

        [Test]
        public void BatchExtension_NoSourceDataEnumerable_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateExtensionTemplate<SimpleExtensionMethodController>(nameof(SimpleExtensionMethodController.NoSourceDataParam));
            });
        }

        [Test]
        public void BatchExtension_MethodAndAttributeDeclaredReturnTypes_ButMistmatched_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateExtensionTemplate<SimpleExtensionMethodController>(nameof(SimpleExtensionMethodController.MismatchedPropertyDeclarations));
            });
        }
    }
}