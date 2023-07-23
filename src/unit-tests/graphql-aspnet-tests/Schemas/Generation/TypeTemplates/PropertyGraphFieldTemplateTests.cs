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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.PropertyTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PropertyGraphFieldTemplateTests
    {
        [Test]
        public void Parse_DescriptionAttribute_SetsValue()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Address1));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual("A Prop Description", template.Description);
        }

        [Test]
        public void Parse_DepreciationAttribute_SetsValues()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Address2));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();
        }

        [Test]
        public void Parse_PropertyAsAnObject_SetsReturnTypeCorrectly()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Hair));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(typeof(SimplePropertyObject.HairData), template.ObjectType);
        }

        [Test]
        public void Parse_PropertyAsAList_SetsReturnType_AsCoreItemNotTheList()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;

            // wigs is List<HairData>
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Wigs));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(typeof(SimplePropertyObject.HairData), template.ObjectType);
        }

        [Test]
        public void Parse_SecurityPolices_AreAdded()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.LastName));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, Enumerable.Count<AppliedSecurityPolicy>(template.SecurityPolicies));
        }

        [Test]
        public void Parse_InvalidName_ThrowsException()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.City));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_SkipDefined_ThrowsException()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.State));

            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_BasicObject_PropertyWithNoGetter_ThrowsException()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(NoGetterOnProperty).GetProperty(nameof(NoGetterOnProperty.Prop1));

            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();

            var ex = Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });

            Assert.IsTrue(ex.Message.Contains("does not define a public getter"));
        }

        [Test]
        public void Parse_BasicObject_PropertyReturnsArray_YieldsTemplate()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var expectedTypeExpression = new GraphTypeExpression(
                Constants.Other.DEFAULT_TYPE_EXPRESSION_TYPE_NAME,
                MetaGraphTypes.IsList);

            var parent = obj.Object;
            var propInfo = typeof(ArrayPropertyObject).GetProperty(nameof(ArrayPropertyObject.PropertyA));

            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(expectedTypeExpression, template.TypeExpression);
            Assert.AreEqual(typeof(TwoPropertyObject[]), template.DeclaredReturnType);
        }

        [Test]
        public void Parse_BasicObject_PropertyReturnsArrayOfKeyValuePair_YieldsTemplate()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var expectedTypeExpression = new GraphTypeExpression(
                Constants.Other.DEFAULT_TYPE_EXPRESSION_TYPE_NAME,  // expression is expected to be unnamed at the template level
                MetaGraphTypes.IsList,
                MetaGraphTypes.IsNotNull); // structs can't be null

            var parent = obj.Object;
            var propInfo = typeof(ArrayKeyValuePairObject).GetProperty(nameof(ArrayKeyValuePairObject.PropertyA));

            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(expectedTypeExpression, template.TypeExpression);
            Assert.AreEqual(typeof(KeyValuePair<string, string>[]), template.DeclaredReturnType);
        }

        [Test]
        public void Parse_AssignedDirective_IsTemplatized()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var expectedTypeExpression = new GraphTypeExpression(
                typeof(KeyValuePair<string, string>).FriendlyGraphTypeName(),
                MetaGraphTypes.IsList,
                MetaGraphTypes.IsNotNull); // structs can't be null

            var parent = obj.Object;
            var propInfo = typeof(PropertyClassWithDirective).GetProperty(nameof(PropertyClassWithDirective.Prop1));

            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();
            Assert.AreEqual(1, Enumerable.Count<IAppliedDirectiveTemplate>(template.AppliedDirectives));

            var appliedDirective = Enumerable.First<IAppliedDirectiveTemplate>(template.AppliedDirectives);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            Assert.AreEqual(new object[] { 55, "property arg" }, appliedDirective.Arguments);
        }

        [Test]
        public void InvalidTypeExpression_ThrowsException()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.InvalidTypeExpression));

            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_InternalName_IsSetCorrectly()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalName).Returns("Item0");

            var expectedTypeExpression = new GraphTypeExpression(
                typeof(KeyValuePair<string, string>).FriendlyGraphTypeName(),
                MetaGraphTypes.IsList,
                MetaGraphTypes.IsNotNull); // structs can't be null

            var parent = obj.Object;
            var propInfo = typeof(PropertyWithInternalName).GetProperty(nameof(PropertyWithInternalName.Prop1));

            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();
            Assert.AreEqual("prop_Field_223", template.InternalName);
        }
    }
}