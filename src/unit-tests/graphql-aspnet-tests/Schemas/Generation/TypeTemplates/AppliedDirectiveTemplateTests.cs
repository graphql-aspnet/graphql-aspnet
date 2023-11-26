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
    using System;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;

    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class AppliedDirectiveTemplateTests
    {
        [Test]
        public void PropertyCheck()
        {
            var owner = Substitute.For<ISchemaItemTemplate>();
            owner.InternalName.Returns("OWNER");
            var template = new AppliedDirectiveTemplate(
                owner,
                typeof(DirectiveWithArgs),
                1,
                "bob");

            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(typeof(DirectiveWithArgs), template.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 1, "bob" }, template.Arguments);
        }

        [Test]
        public void NulLTemplateType_ThrowsException()
        {
            var owner = Substitute.For<ISchemaItemTemplate>();
            owner.InternalName.Returns("OWNER");
            var template = new AppliedDirectiveTemplate(
                owner,
                null as Type,
                1,
                "bob");

            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void NullName_ThrowsException()
        {
            var owner = Substitute.For<ISchemaItemTemplate>();
            owner.InternalName.Returns("OWNER");
            var template = new AppliedDirectiveTemplate(
                owner,
                null as string,
                1,
                "bob");

            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void EmptyName_ThrowsException()
        {
            var owner = Substitute.For<ISchemaItemTemplate>();
            owner.InternalName.Returns("OWNER");
            var template = new AppliedDirectiveTemplate(
                owner,
                string.Empty,
                1,
                "bob");

            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void NonDirectiveType_ThrowsException()
        {
            var owner = Substitute.For<ISchemaItemTemplate>();
            owner.InternalName.Returns("OWNER");
            var template = new AppliedDirectiveTemplate(
                owner,
                typeof(AppliedDirectiveTemplateTests),
                1,
                "bob");

            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }
    }
}