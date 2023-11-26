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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Common.Interfaces;
    using GraphQL.AspNet.Tests.Internal.Templating.ExtensionMethodTestData;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ExtensionMethodTestData;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class TypeExtensionFieldTemplateTests
    {
        private GraphTypeExtensionFieldTemplate CreateExtensionTemplate<TControllerType>(string actionName)
            where TControllerType : GraphController
        {
            var mockController = Substitute.For<IGraphControllerTemplate>();
            mockController.InternalName.Returns(typeof(TControllerType).Name);
            mockController.Route.Returns(new SchemaItemPath("path0"));
            mockController.Name.Returns("path0");
            mockController.ObjectType.Returns(typeof(TControllerType));

            var methodInfo = typeof(TControllerType).GetMethod(actionName);
            var template = new GraphTypeExtensionFieldTemplate(mockController, methodInfo);
            template.Parse();
            template.ValidateOrThrow();

            return template;
        }

        [Test]
        public void ClassTypeExtension_PropertyCheck()
        {
            var methodInfo = typeof(ExtensionMethodController).GetMethod(nameof(ExtensionMethodController.ClassTypeExtension));
            var template = this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.ClassTypeExtension));

            Assert.AreEqual("ClassTypeExtensionDescription", template.Description);
            Assert.AreEqual(SchemaItemCollections.Types, template.Route.RootCollection);
            Assert.AreEqual(typeof(ExtensionMethodController), template.Parent.ObjectType);
            Assert.AreEqual(typeof(TwoPropertyObject), template.SourceObjectType);
            Assert.AreEqual($"[type]/{nameof(TwoPropertyObject)}/Property3", template.Route.Path);
            Assert.AreEqual($"{nameof(ExtensionMethodController)}.{nameof(ExtensionMethodController.ClassTypeExtension)}", template.InternalName);
            Assert.AreEqual(methodInfo.ReflectedType, template.Parent.ObjectType);
            Assert.AreEqual("path0", template.Parent.Name);
            Assert.AreEqual(methodInfo, template.Method);
            Assert.AreEqual(typeof(TwoPropertyObjectV2), template.ObjectType);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual(0, template.TypeExpression.Wrappers.Length);
            Assert.IsFalse(template.Route.IsTopLevelField);
            Assert.IsFalse(template.IsAsyncField);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, template.Mode);

            // first arg should be declared for the source data
            Assert.IsTrue(template.Arguments[0].ArgumentModifier.HasFlag(GraphArgumentModifiers.ParentFieldResult));
        }

        [Test]
        public void ClassBatchExtension_PropertyCheck()
        {
            var methodInfo = typeof(ExtensionMethodController).GetMethod(nameof(ExtensionMethodController.ClassBatchExtension));
            var template = this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.ClassBatchExtension));

            Assert.AreEqual("ClassBatchExtensionDescription", template.Description);
            Assert.AreEqual(methodInfo.ReflectedType, template.Parent.ObjectType);
            Assert.AreEqual(typeof(TwoPropertyObject), template.SourceObjectType);
            Assert.AreEqual("path0", template.Parent.Name);
            Assert.AreEqual(methodInfo, template.Method);
            Assert.AreEqual(typeof(int), template.ObjectType);
            CollectionAssert.AreEqual(new[] { MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull }, template.TypeExpression.Wrappers);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual(FieldResolutionMode.Batch, template.Mode);

            // first arg should be declared for the source data
            Assert.IsTrue(template.Arguments[0].ArgumentModifier.HasFlag(GraphArgumentModifiers.ParentFieldResult));
        }

        [Test]
        public void StructTypeExtension_PropertyCheck()
        {
            var methodInfo = typeof(ExtensionMethodController).GetMethod(nameof(ExtensionMethodController.StructTypeExtension));
            var template = this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.StructTypeExtension));

            Assert.AreEqual("StructTypeExtensionDescription", template.Description);
            Assert.AreEqual(SchemaItemCollections.Types, template.Route.RootCollection);
            Assert.AreEqual(typeof(ExtensionMethodController), template.Parent.ObjectType);
            Assert.AreEqual(typeof(TwoPropertyStruct), template.SourceObjectType);
            Assert.AreEqual($"[type]/{nameof(TwoPropertyStruct)}/Property3", template.Route.Path);
            Assert.AreEqual($"{nameof(ExtensionMethodController)}.{nameof(ExtensionMethodController.StructTypeExtension)}", template.InternalName);
            Assert.AreEqual(methodInfo.ReflectedType, template.Parent.ObjectType);
            Assert.AreEqual("path0", template.Parent.Name);
            Assert.AreEqual(methodInfo, template.Method);
            Assert.AreEqual(typeof(TwoPropertyObjectV2), template.ObjectType);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual(0, template.TypeExpression.Wrappers.Length);
            Assert.IsFalse(template.Route.IsTopLevelField);
            Assert.IsFalse(template.IsAsyncField);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, template.Mode);

            // first arg should be declared for the source data
            Assert.IsTrue(template.Arguments[0].ArgumentModifier.HasFlag(GraphArgumentModifiers.ParentFieldResult));
        }

        [Test]
        public void StructBatchExtension_PropertyCheck()
        {
            var methodInfo = typeof(ExtensionMethodController).GetMethod(nameof(ExtensionMethodController.StructBatchTestExtension));
            var template = this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.StructBatchTestExtension));

            Assert.AreEqual("StructBatchExtensionDescription", template.Description);
            Assert.AreEqual(methodInfo.ReflectedType, template.Parent.ObjectType);
            Assert.AreEqual(typeof(TwoPropertyStruct), template.SourceObjectType);
            Assert.AreEqual("path0", template.Parent.Name);
            Assert.AreEqual(methodInfo, template.Method);
            Assert.AreEqual(typeof(int), template.ObjectType);
            CollectionAssert.AreEqual(new[] { MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull }, template.TypeExpression.Wrappers);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual(FieldResolutionMode.Batch, template.Mode);

            // first arg should be declared for the source data
            Assert.IsTrue(template.Arguments[0].ArgumentModifier.HasFlag(GraphArgumentModifiers.ParentFieldResult));
        }

        [Test]
        public void InterfaceTypeExtension_PropertyCheck()
        {
            var methodInfo = typeof(ExtensionMethodController).GetMethod(nameof(ExtensionMethodController.InterfaceTypeExtension));
            var template = this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.InterfaceTypeExtension));

            Assert.AreEqual("InterfaceTypeExtensionDescription", template.Description);
            Assert.AreEqual(SchemaItemCollections.Types, template.Route.RootCollection);
            Assert.AreEqual(typeof(ExtensionMethodController), template.Parent.ObjectType);
            Assert.AreEqual(typeof(ISinglePropertyObject), template.SourceObjectType);
            Assert.AreEqual($"[type]/TwoPropertyInterface/Property3", template.Route.Path);
            Assert.AreEqual($"{nameof(ExtensionMethodController)}.{nameof(ExtensionMethodController.InterfaceTypeExtension)}", template.InternalName);
            Assert.AreEqual(methodInfo.ReflectedType, template.Parent.ObjectType);
            Assert.AreEqual("path0", template.Parent.Name);
            Assert.AreEqual(methodInfo, template.Method);
            Assert.AreEqual(typeof(TwoPropertyObjectV2), template.ObjectType);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual(0, template.TypeExpression.Wrappers.Length);
            Assert.IsFalse(template.Route.IsTopLevelField);
            Assert.IsFalse(template.IsAsyncField);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, template.Mode);

            // first arg should be declared for the source data
            Assert.IsTrue(template.Arguments[0].ArgumentModifier.HasFlag(GraphArgumentModifiers.ParentFieldResult));
        }

        [Test]
        public void InterfaceBatchExtension_PropertyCheck()
        {
            var methodInfo = typeof(ExtensionMethodController).GetMethod(nameof(ExtensionMethodController.InterfaceBatchTestExtension));
            var template = this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.InterfaceBatchTestExtension));

            Assert.AreEqual("InterfaceBatchExtensionDescription", template.Description);
            Assert.AreEqual(methodInfo.ReflectedType, template.Parent.ObjectType);
            Assert.AreEqual(typeof(ISinglePropertyObject), template.SourceObjectType);
            Assert.AreEqual("path0", template.Parent.Name);
            Assert.AreEqual(methodInfo, template.Method);
            Assert.AreEqual(typeof(int), template.ObjectType);
            CollectionAssert.AreEqual(new[] { MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull }, template.TypeExpression.Wrappers);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual(FieldResolutionMode.Batch, template.Mode);

            // first arg should be declared for the source data
            Assert.IsTrue(template.Arguments[0].ArgumentModifier.HasFlag(GraphArgumentModifiers.ParentFieldResult));
        }

        [Test]
        public void ValidBatchExtension_WithCustomReturnType_AndNoDeclaredTypeOnAttribute_PropertyCheck()
        {
            var methodInfo = typeof(ExtensionMethodController).GetMethod(nameof(ExtensionMethodController.CustomValidReturnType));
            var template = this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.CustomValidReturnType));

            Assert.AreEqual(methodInfo.ReflectedType, template.Parent.ObjectType);
            Assert.AreEqual(typeof(TwoPropertyObject), template.SourceObjectType);
            Assert.AreEqual(methodInfo, template.Method);
            CollectionAssert.AreEqual(new[] { MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull }, template.TypeExpression.Wrappers);
            Assert.AreEqual(typeof(int), template.ObjectType);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual(FieldResolutionMode.Batch, template.Mode);
        }

        [Test]
        public void ValidBatchExtension_WithCustomNamedReturnType_PropertyCheck()
        {
            var methodInfo = typeof(ExtensionMethodController).GetMethod(nameof(ExtensionMethodController.Batch_CustomNamedObjectReturnedTestExtension));
            var template = this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.Batch_CustomNamedObjectReturnedTestExtension));

            Assert.AreEqual(methodInfo.ReflectedType, template.CreateResolver().MetaData.ParentObjectType);
            Assert.AreEqual(typeof(TwoPropertyObject), template.SourceObjectType);
            Assert.AreEqual(methodInfo, template.Method);
            Assert.AreEqual("Type", template.TypeExpression.ToString());
            Assert.AreEqual("[type]/TwoPropertyObject/fieldThree", template.Route.ToString());
            Assert.AreEqual(typeof(CustomNamedObject), template.ObjectType);
            Assert.AreEqual(1, template.Arguments.Count);
            Assert.AreEqual(FieldResolutionMode.Batch, template.Mode);
        }

        [Test]
        public void ValidBatchExtension_WithCustomNamedReturnType_OnSameCustomNamedParent_PropertyCheck()
        {
            var methodInfo = typeof(ExtensionMethodController).GetMethod(nameof(ExtensionMethodController.Batch_ChildIsSameCustomNamedObjectTestExtension));
            var template = this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.Batch_ChildIsSameCustomNamedObjectTestExtension));

            Assert.AreEqual(methodInfo.ReflectedType, template.CreateResolver().MetaData.ParentObjectType);
            Assert.AreEqual(typeof(CustomNamedObject), template.SourceObjectType);
            Assert.AreEqual(methodInfo, template.Method);
            Assert.AreEqual("Type", template.TypeExpression.ToString());
            Assert.AreEqual("[type]/Custom_Named_Object/fieldThree", template.Route.ToString());
            Assert.AreEqual(typeof(CustomNamedObject), template.ObjectType);
            Assert.AreEqual(1, template.Arguments.Count);
            Assert.AreEqual(FieldResolutionMode.Batch, template.Mode);
        }

        [Test]
        public void BatchExtension_NoSourceDataEnumerable_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.BatchExensionNoSourceDataParam));
            });
        }

        [Test]
        public void BatchExtension_MethodAndAttributeDeclaredReturnTypes_ButMistmatched_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.MismatchedPropertyDeclarations));
            });
        }

        [Test]
        public void TypeExtension_ExtendingAScalar_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.ScalarTypeExtensionFails));
            });
        }

        [Test]
        public void BatchExtension_ExtendingAScalar_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.ScalarBatchExtensionFails));
            });
        }

        [Test]
        public void TypeExtension_ExtendingAnEnum_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.EnumTypeExtensionFails));
            });
        }

        [Test]
        public void BatchExtension_ExtendingAnEnum_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.EnumBatchExtensionFails));
            });
        }

        [Test]
        public void ValidBatchExtension_WithCustomInternalName_PropertyCheck()
        {
            var methodInfo = typeof(ExtensionMethodController).GetMethod(nameof(ExtensionMethodController.CustomeInternalName));
            var template = this.CreateExtensionTemplate<ExtensionMethodController>(nameof(ExtensionMethodController.CustomeInternalName));

            Assert.AreEqual("BatchInternalName", template.InternalName);
        }
    }
}