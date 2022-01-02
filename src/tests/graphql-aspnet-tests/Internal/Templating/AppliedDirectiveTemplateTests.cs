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
    using System;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class AppliedDirectiveTemplateTests
    {
        [Test]
        public void PropertyCheck()
        {
            var owner = new Mock<IGraphItemTemplate>();
            owner.Setup(x => x.InternalFullName).Returns("OWNER");
            var template = new AppliedDirectiveTemplate(
                owner.Object,
                typeof(DirectiveWithArgs),
                1,
                "bob");

            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(owner.Object, template.Owner);
            Assert.AreEqual(typeof(DirectiveWithArgs), template.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 1, "bob" }, template.Arguments);
        }

        [Test]
        public void NulLTemplateType_ThrowsException()
        {
            var owner = new Mock<IGraphItemTemplate>();
            owner.Setup(x => x.InternalFullName).Returns("OWNER");
            var template = new AppliedDirectiveTemplate(
                owner.Object,
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
            var owner = new Mock<IGraphItemTemplate>();
            owner.Setup(x => x.InternalFullName).Returns("OWNER");
            var template = new AppliedDirectiveTemplate(
                owner.Object,
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
            var owner = new Mock<IGraphItemTemplate>();
            owner.Setup(x => x.InternalFullName).Returns("OWNER");
            var template = new AppliedDirectiveTemplate(
                owner.Object,
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
            var owner = new Mock<IGraphItemTemplate>();
            owner.Setup(x => x.InternalFullName).Returns("OWNER");
            var template = new AppliedDirectiveTemplate(
                owner.Object,
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