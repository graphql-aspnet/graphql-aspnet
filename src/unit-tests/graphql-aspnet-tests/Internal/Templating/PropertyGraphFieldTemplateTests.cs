﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;
    using GraphQL.AspNet.Tests.Internal.Templating.PropertyTestData;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class PropertyGraphFieldTemplateTests
    {
        [Test]
        public void Parse_DescriptionAttribute_SetsValue()
        {
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var parent = obj;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Address1));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual("A Prop Description", template.Description);
        }

        [Test]
        public void Parse_DepreciationAttribute_SetsValues()
        {
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var parent = obj;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Address2));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();
        }

        [Test]
        public void Parse_PropertyAsAnObject_SetsReturnTypeCorrectly()
        {
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var parent = obj;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Hair));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(typeof(SimplePropertyObject.HairData), template.ObjectType);
        }

        [Test]
        public void Parse_PropertyAsAList_SetsReturnType_AsCoreItemNotTheList()
        {
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var parent = obj;

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
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var parent = obj;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.LastName));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.SecurityPolicies.Count());
        }

        [Test]
        public void Parse_InvalidName_ThrowsException()
        {
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var parent = obj;
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
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var parent = obj;
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
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var parent = obj;
            var propInfo = typeof(NoGetterOnProperty).GetProperty(nameof(NoGetterOnProperty.Prop1));

            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_BasicObject_PropertyReturnsArray_YieldsTemplate()
        {
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var expectedTypeExpression = new GraphTypeExpression(
                typeof(TwoPropertyObject).FriendlyName(),
                MetaGraphTypes.IsList);

            var parent = obj;
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
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var expectedTypeExpression = new GraphTypeExpression(
                typeof(KeyValuePair<string, string>).FriendlyGraphTypeName(),
                MetaGraphTypes.IsList,
                MetaGraphTypes.IsNotNull); // structs can't be null

            var parent = obj;
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
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var expectedTypeExpression = new GraphTypeExpression(
                typeof(KeyValuePair<string, string>).FriendlyGraphTypeName(),
                MetaGraphTypes.IsList,
                MetaGraphTypes.IsNotNull); // structs can't be null

            var parent = obj;
            var propInfo = typeof(PropertyClassWithDirective).GetProperty(nameof(PropertyClassWithDirective.Prop1));

            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();
            Assert.AreEqual(1, template.AppliedDirectives.Count());

            var appliedDirective = template.AppliedDirectives.First();
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            Assert.AreEqual(new object[] { 55, "property arg" }, appliedDirective.Arguments);
        }

        [Test]
        public void InvalidTypeExpression_ThrowsException()
        {
            var obj = Substitute.For<IObjectGraphTypeTemplate>();
            obj.Route.Returns(new SchemaItemPath("[type]/Item0"));
            obj.InternalFullName.Returns("Item0");

            var parent = obj;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.InvalidTypeExpression));

            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }
    }
}