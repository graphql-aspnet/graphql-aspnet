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
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Internal.Templating.PropertyTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GraphPropertyTemplateTests
    {
        [Test]
        public void Parse_DescriptionAttribute_SetsValue()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

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
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Address2));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsTrue(template.IsDeprecated);
            Assert.AreEqual("A Reason", template.DeprecationReason);
        }

        [Test]
        public void Parse_PropertyAsAnObject_SetsReturnTypeCorrectly()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

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
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

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
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.LastName));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.SecurityPolicies.Count());
        }

        [Test]
        public void Parse_InvalidName_ThrowsException()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

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
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

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
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(NoGetterOnProperty).GetProperty(nameof(NoGetterOnProperty.Prop1));

            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }
    }
}