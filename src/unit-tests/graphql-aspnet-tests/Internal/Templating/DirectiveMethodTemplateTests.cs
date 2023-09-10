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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class DirectiveMethodTemplateTests
    {
        [Test]
        public void SimpleDescriptor_AllDefaults_GeneralPropertyCheck()
        {
            var method = typeof(SimpleExecutableDirective)
                .GetMethod(nameof(SimpleExecutableDirective.Execute));

            var mock = Substitute.For<IGraphTypeTemplate>();
            mock.InternalFullName.Returns("Simple");
            var route = new SchemaItemPath(SchemaItemCollections.Directives, "Simple");
            mock.Route.Returns(route);
            var template = new GraphDirectiveMethodTemplate(mock, method);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(mock, template.Parent);
            Assert.AreEqual(method, template.Method);

            Assert.AreEqual("IGraphActionResult (int arg1, string arg2)", template.MethodSignature);
            Assert.AreEqual(nameof(SimpleExecutableDirective.Execute), template.Name);
            Assert.AreEqual($"Simple.{nameof(SimpleExecutableDirective.Execute)}", template.InternalFullName);
            Assert.IsTrue(template.IsAsyncField);
            Assert.AreEqual(typeof(IGraphActionResult), template.ObjectType);
            Assert.AreEqual(DirectiveLocation.FIELD, template.Locations);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual("arg1", template.Arguments[0].Name);
            Assert.AreEqual(typeof(int), template.Arguments[0].ObjectType);
            Assert.AreEqual("arg2", template.Arguments[1].Name);
            Assert.AreEqual(typeof(string), template.Arguments[1].ObjectType);
            Assert.IsTrue(template.IsExplicitDeclaration);
            Assert.AreEqual(GraphFieldSource.Method, template.FieldSource);

            Assert.AreEqual(null, template.DeclaredTypeWrappers);
        }

        [Test]
        public void MethodWithSkipAttached_ThrowsException()
        {
            // afterfieldResolution is marked skipped
            var method = typeof(TestDirectiveMethodTemplateContainer)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer.SkippedMethod));

            var mock = Substitute.For<IGraphTypeTemplate>();
            mock.InternalFullName.Returns("Simple");
            var route = new SchemaItemPath(SchemaItemCollections.Directives, "Simple");
            mock.Route.Returns(route);
            var template = new GraphDirectiveMethodTemplate(mock, method);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void InvalidReturnType_ThrowsException()
        {
            var method = typeof(TestDirectiveMethodTemplateContainer)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer.IncorrrectReturnType));

            var mock = Substitute.For<IGraphTypeTemplate>();
            mock.InternalFullName.Returns("Simple");
            var route = new SchemaItemPath(SchemaItemCollections.Directives, "Simple");
            mock.Route.Returns(route);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new GraphDirectiveMethodTemplate(mock, method);
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void AsyncMethodWithNoReturnType_ThrowsException()
        {
            var method = typeof(TestDirectiveMethodTemplateContainer2)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer2.InvalidTaskReference));

            var mock = Substitute.For<IGraphTypeTemplate>();
            mock.InternalFullName.Returns("Simple");
            var route = new SchemaItemPath(SchemaItemCollections.Directives, "Simple");
            mock.Route.Returns(route);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new GraphDirectiveMethodTemplate(mock, method);
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void InterfaceAsInputParameter_ThrowsException()
        {
            var method = typeof(TestDirectiveMethodTemplateContainer2)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer2.InterfaceAsParameter));

            var mock = Substitute.For<IGraphTypeTemplate>();
            mock.InternalFullName.Returns("Simple");
            var route = new SchemaItemPath(SchemaItemCollections.Directives, "Simple");
            mock.Route.Returns(route);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new GraphDirectiveMethodTemplate(mock, method);
                template.Parse();
                template.ValidateOrThrow();
            });
        }
    }
}