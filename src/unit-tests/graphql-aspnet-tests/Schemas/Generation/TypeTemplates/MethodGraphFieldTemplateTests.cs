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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.MethodTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MethodGraphFieldTemplateTests
    {
        private MethodGraphFieldTemplate CreateMethodTemplate<TObject>(string methodName)
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;
            var methodInfo = typeof(TObject).GetMethod(methodName);
            var template = new MethodGraphFieldTemplate(parent, methodInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();
            return template;
        }

        [Test]
        public void DefaultValuesCheck()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;
            var methodInfo = typeof(MethodClass).GetMethod(nameof(MethodClass.SimpleMethodNoAttributes));
            var template = new MethodGraphFieldTemplate(parent, methodInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(nameof(MethodClass.SimpleMethodNoAttributes), template.Name);
            Assert.AreEqual($"Item0.{nameof(MethodClass.SimpleMethodNoAttributes)}", template.InternalName);
            Assert.AreEqual($"[type]/Item0/{nameof(MethodClass.SimpleMethodNoAttributes)}", template.Route.Path);
            Assert.AreEqual(parent.Route.Path, template.Route.Parent.Path);
            Assert.AreEqual(methodInfo, template.Method);
            Assert.AreEqual(parent, template.Parent);
            Assert.AreEqual(0, template.TypeExpression.Wrappers.Length);
        }

        [Test]
        public void ExplicitFieldTypeOptions_SetsFieldOptionsCorrectly()
        {
            var template = this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.ForceNotNull));
            CollectionAssert.AreEqual(new[] { MetaGraphTypes.IsNotNull }, template.TypeExpression.Wrappers);
        }

        [Test]
        public void GraphNameAttribute_SetsDataCorrectly()
        {
            var template = this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.overriddenName));
            Assert.AreEqual("SuperName", template.Name);
        }

        [Test]
        public void DescriptionAttribute_SetsDataCorrectly()
        {
            var template = this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.DescriptiionMethod));
            Assert.AreEqual("A Valid Description", template.Description);
        }

        [Test]
        public void ParameterParsing_SetsParamsCorrectly()
        {
            var template = this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.SimpleParameterCheck));
            Assert.AreEqual(1, template.Arguments.Count);

            var param = template.Arguments[0];
            Assert.AreEqual("arg1", param.Name);
            Assert.AreEqual(typeof(int), param.ObjectType);
            CollectionAssert.AreEqual(new[] { MetaGraphTypes.IsNotNull }, param.TypeExpression.Wrappers);
        }

        [Test]
        public void AlternateNameParameterParsing_SetsParamsCorrectly()
        {
            var template = this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.ParamWithAlternateName));

            Assert.AreEqual(1, template.Arguments.Count);
            var param = template.Arguments[0];
            Assert.AreEqual("arg55", param.Name);
            Assert.AreEqual("Item0.ParamWithAlternateName.arg1", param.InternalName);
        }

        [Test]
        public void PropertyAsAList_SetsOptionsCorrectly()
        {
            var template = this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.ListOfObjectReturnType));
            CollectionAssert.AreEqual(new[] { MetaGraphTypes.IsList }, template.TypeExpression.Wrappers);
        }

        [Test]
        public void InvalidParameterName_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.InvalidParameterName));
            });
        }

        [Test]
        public void AsyncMethodWithNoReturnType_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.AsyncMethodNoReturnType));
            });
        }

        [Test]
        public void PropertyAsAnObject_SetsReturnTypeCorrectly()
        {
            var template = this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.ObjectReturnType));
            Assert.AreEqual(typeof(TwoPropertyObject), template.ObjectType);
        }

        [Test]
        public void PropertyAsAList_SetsReturnType_AsCoreItemNotTheList()
        {
            var template = this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.ListOfObjectReturnType));
            Assert.AreEqual(typeof(TwoPropertyObject), template.ObjectType);
        }

        [Test]
        public void PropertyAsATaskOfList_SetsReturnType_AsCoreItemNotTheList()
        {
            var template = this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.TaskOfListOfObjectReturnType));
            Assert.AreEqual(typeof(TwoPropertyObject), template.ObjectType);
        }

        [Test]
        public void ArrayFromMethod_YieldsTemplate()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var expectedTypeExpression = new GraphTypeExpression(
                Constants.Other.DEFAULT_TYPE_EXPRESSION_TYPE_NAME,
                MetaGraphTypes.IsList);

            var parent = obj.Object;
            var template = this.CreateMethodTemplate<ArrayMethodObject>(nameof(ArrayMethodObject.RetrieveData));

            Assert.AreEqual(expectedTypeExpression, template.TypeExpression);
            Assert.AreEqual(typeof(TwoPropertyObject[]), template.DeclaredReturnType);
        }

        [Test]
        public void Parse_AssignedDirective_IsTemplatized()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var expectedTypeExpression = new GraphTypeExpression(
                typeof(TwoPropertyObject).FriendlyName(),
                MetaGraphTypes.IsList);

            var parent = obj.Object;
            var template = this.CreateMethodTemplate<MethodClassWithDirective>(nameof(MethodClassWithDirective.Counter));

            Assert.AreEqual(1, template.AppliedDirectives.Count());

            var appliedDirective = template.AppliedDirectives.First();
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            Assert.AreEqual(new object[] { 44, "method arg" }, appliedDirective.Arguments);
        }

        [Test]
        public void DefaultNonNullableParameter_NotMarkedRequired()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;
            var template = this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.DefaultNonNullableParameter));

            Assert.AreEqual(0, template.AppliedDirectives.Count());

            Assert.AreEqual(1, template.Arguments.Count);

            var arg = template.Arguments[0];
            Assert.IsTrue(arg.HasDefaultValue);
            Assert.AreEqual(5, arg.DefaultValue);
        }

        [Test]
        public void InvalidTypeExpression_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                this.CreateMethodTemplate<MethodClass>(nameof(MethodClass.InvalidTypeExpression));
            });
        }
    }
}