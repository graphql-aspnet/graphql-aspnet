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
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GraphDirectiveMethodTemplateTests
    {
        [Test]
        public void SimpleDescriptor_AllDefaults_GeneralPropertyCheck()
        {
            var method = typeof(SimpleExecutableDirective).GetMethod(nameof(SimpleExecutableDirective.BeforeFieldResolution));

            var mock = new Mock<IGraphTypeTemplate>();
            mock.Setup(x => x.InternalFullName).Returns("Simple");
            var route = new GraphFieldPath(GraphCollection.Directives, "Simple");
            mock.Setup(x => x.Route).Returns(route);
            var template = new GraphDirectiveMethodTemplate(mock.Object, method);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(mock.Object, template.Parent);
            Assert.AreEqual(method, template.Method);

            Assert.AreEqual("IGraphActionResult (int arg1, string arg2)", template.MethodSignature);
            Assert.AreEqual(nameof(SimpleExecutableDirective.BeforeFieldResolution), template.Name);
            Assert.AreEqual($"Simple.{nameof(SimpleExecutableDirective.BeforeFieldResolution)}", template.InternalFullName);
            Assert.IsTrue(template.IsValidDirectiveMethodName);
            Assert.IsTrue(template.IsValidDirectiveMethodSignature);
            Assert.IsTrue(template.IsAsyncField);
            Assert.AreEqual(typeof(IGraphActionResult), template.ObjectType);
            Assert.AreEqual(DirectiveLifeCycleEvent.BeforeResolution, template.LifeCycleEvent);
            Assert.AreEqual(2, template.Arguments.Count);
            Assert.AreEqual("arg1", template.Arguments[0].Name);
            Assert.AreEqual(typeof(int), template.Arguments[0].ObjectType);
            Assert.AreEqual("arg2", template.Arguments[1].Name);
            Assert.AreEqual(typeof(string), template.Arguments[1].ObjectType);
            Assert.IsTrue(template.IsExplicitDeclaration);
            Assert.AreEqual(GraphFieldSource.Method, template.FieldSource);

            var declarationExpression = template as IGraphTypeExpressionDeclaration;
            Assert.AreEqual(false, declarationExpression.HasDefaultValue);
            Assert.AreEqual(null, declarationExpression.TypeWrappers);
        }

        [Test]
        public void MethodWithSkipAttached_ThrowsException()
        {
            // afterfieldResolution is marked skipped
            var method = typeof(TestDirectiveMethodTemplateContainer)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer.AfterFieldResolution));

            var mock = new Mock<IGraphTypeTemplate>();
            mock.Setup(x => x.InternalFullName).Returns("Simple");
            var route = new GraphFieldPath(GraphCollection.Directives, "Simple");
            mock.Setup(x => x.Route).Returns(route);
            var template = new GraphDirectiveMethodTemplate(mock.Object, method);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void NotADirectiveMethod_ThrowsException()
        {
            var method = typeof(TestDirectiveMethodTemplateContainer)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer.NotADirectiveMethod));

            var mock = new Mock<IGraphTypeTemplate>();
            mock.Setup(x => x.InternalFullName).Returns("Simple");
            var route = new GraphFieldPath(GraphCollection.Directives, "Simple");
            mock.Setup(x => x.Route).Returns(route);
            var template = new GraphDirectiveMethodTemplate(mock.Object, method);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void InvalidMethodSignature_IndicatesInproperty()
        {
            var method = typeof(TestDirectiveMethodTemplateContainer)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer.AfterFieldResolution));

            var mock = new Mock<IGraphTypeTemplate>();
            mock.Setup(x => x.InternalFullName).Returns("Simple");
            var route = new GraphFieldPath(GraphCollection.Directives, "Simple");
            mock.Setup(x => x.Route).Returns(route);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new GraphDirectiveMethodTemplate(mock.Object, method);
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void ValidLifeCycleMethod_WithGraphSkip_ThrowsException()
        {
            var method = typeof(TestDirectiveMethodTemplateContainer2)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer2.BeforeFieldResolution));

            var mock = new Mock<IGraphTypeTemplate>();
            mock.Setup(x => x.InternalFullName).Returns("Simple");
            var route = new GraphFieldPath(GraphCollection.Directives, "Simple");
            mock.Setup(x => x.Route).Returns(route);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new GraphDirectiveMethodTemplate(mock.Object, method);
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void InvalidValidLifeCycleMethodName_ThrowsException()
        {
            var method = typeof(TestDirectiveMethodTemplateContainer2)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer2.NotAValidMethodName));

            var mock = new Mock<IGraphTypeTemplate>();
            mock.Setup(x => x.InternalFullName).Returns("Simple");
            var route = new GraphFieldPath(GraphCollection.Directives, "Simple");
            mock.Setup(x => x.Route).Returns(route);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new GraphDirectiveMethodTemplate(mock.Object, method);
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void InvalidTaskReferenceOnMethod_ThrowsException()
        {
            var method = typeof(TestDirectiveMethodTemplateContainer2)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer2.AfterFieldResolution));

            var mock = new Mock<IGraphTypeTemplate>();
            mock.Setup(x => x.InternalFullName).Returns("Simple");
            var route = new GraphFieldPath(GraphCollection.Directives, "Simple");
            mock.Setup(x => x.Route).Returns(route);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new GraphDirectiveMethodTemplate(mock.Object, method);
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void AlterTypeSystemMethod_DoesNotHaveInputParameter_ThrowsException()
        {
            var method = typeof(TestDirectiveMethodTemplateContainer2)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer2.AlterTypeSystem));

            var mock = new Mock<IGraphTypeTemplate>();
            mock.Setup(x => x.InternalFullName).Returns("Simple");
            var route = new GraphFieldPath(GraphCollection.Directives, "Simple");
            mock.Setup(x => x.Route).Returns(route);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new GraphDirectiveMethodTemplate(mock.Object, method);
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void AlterTypeSystemMethod_HasWrongInputParameter_ThrowsException()
        {
            var method = typeof(TestDirectiveMethodTemplateContainer3)
                .GetMethod(nameof(TestDirectiveMethodTemplateContainer3.AlterTypeSystem));

            var mock = new Mock<IGraphTypeTemplate>();
            mock.Setup(x => x.InternalFullName).Returns("Simple");
            var route = new GraphFieldPath(GraphCollection.Directives, "Simple");
            mock.Setup(x => x.Route).Returns(route);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new GraphDirectiveMethodTemplate(mock.Object, method);
                template.Parse();
                template.ValidateOrThrow();
            });
        }
    }
}