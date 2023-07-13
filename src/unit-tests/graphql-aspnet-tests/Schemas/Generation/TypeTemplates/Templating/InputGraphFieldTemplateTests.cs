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
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Internal.Templating.PropertyTestData;
    using Moq;
    using NUnit.Framework;

    public class InputGraphFieldTemplateTests
    {
        [Test]
        public void Parse_GeneralPropertyCheck()
        {
            var obj = new Mock<IInputObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");
            obj.Setup(x => x.Kind).Returns(TypeKind.INPUT_OBJECT);

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Name));
            var template = new InputGraphFieldTemplate(parent, propInfo);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual("Name", template.Name);
            Assert.AreEqual("[type]/Item0/Name", template.Route.ToString());
            Assert.AreEqual("name desc", template.Description);
            Assert.AreEqual(typeof(string), template.ObjectType);
            Assert.IsFalse(template.IsRequired);
            Assert.AreEqual("String", template.TypeExpression.ToString());
            Assert.AreEqual(1, template.AppliedDirectives.Count());
            Assert.AreEqual("nameDirective", template.AppliedDirectives.Single().DirectiveName);
            Assert.AreEqual("Name", template.InternalName);
        }

        [Test]
        public void Parse_IsRequired_IsNotSet()
        {
            var obj = new Mock<IInputObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");
            obj.Setup(x => x.Kind).Returns(TypeKind.INPUT_OBJECT);

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Age));
            var template = new InputGraphFieldTemplate(parent, propInfo);
            template.Parse();
            template.ValidateOrThrow();

            // field does not declare [Required] therefore must
            // have a default value
            Assert.IsFalse(template.IsRequired);
        }

        [Test]
        public void Parse_IsRequired_IsSet()
        {
            var obj = new Mock<IInputObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");
            obj.Setup(x => x.Kind).Returns(TypeKind.INPUT_OBJECT);

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.RequiredAge));
            var template = new InputGraphFieldTemplate(parent, propInfo);
            template.Parse();
            template.ValidateOrThrow();

            // field does declare [Required] there for cannot have a default value
            Assert.IsTrue(template.IsRequired);
        }

        [Test]
        public void Parse_NotRequired_NullableType_ButWithExplicitNonNullTypeExpression_IsFlaggedNonNullable()
        {
            var obj = new Mock<IInputObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");
            obj.Setup(x => x.Kind).Returns(TypeKind.INPUT_OBJECT);

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Shoes));
            var template = new InputGraphFieldTemplate(parent, propInfo);
            template.Parse();
            template.ValidateOrThrow();

            // field does declare [Required] there for cannot have a default value
            Assert.AreEqual("Input_ShoeData!", template.TypeExpression.ToString());
        }

        [Test]
        public void Parse_InterfaceAsPropertyType_ThrowsException()
        {
            var obj = new Mock<IInputObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");
            obj.Setup(x => x.Kind).Returns(TypeKind.INPUT_OBJECT);

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.InterfaceProperty));
            var template = new InputGraphFieldTemplate(parent, propInfo);
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_TaskAsPropertyType_ThrowsException()
        {
            var obj = new Mock<IInputObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");
            obj.Setup(x => x.Kind).Returns(TypeKind.INPUT_OBJECT);

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.TaskProperty));
            var template = new InputGraphFieldTemplate(parent, propInfo);
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_UnionAsPropertyType_ThrowsException()
        {
            var obj = new Mock<IInputObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");
            obj.Setup(x => x.Kind).Returns(TypeKind.INPUT_OBJECT);

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.UnionProxyProperty));
            var template = new InputGraphFieldTemplate(parent, propInfo);
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_ActionResultAsPropertyType_ThrowsException()
        {
            var obj = new Mock<IInputObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");
            obj.Setup(x => x.Kind).Returns(TypeKind.INPUT_OBJECT);

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.ActionResultProperty));
            var template = new InputGraphFieldTemplate(parent, propInfo);
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }
    }
}