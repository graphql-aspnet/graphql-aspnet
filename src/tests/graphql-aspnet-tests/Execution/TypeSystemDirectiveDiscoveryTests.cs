// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution
{
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTestData;
    using GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTests;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class TypeSystemDirectiveDiscoveryTests
    {
        [Test]
        public void DirectiveDeclaredByName_WhenNotExplicitlyIncluded_ThrowsStartupException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                    .AddGraphType<TestPersonWithResolverExtensionDirectiveByName>()
                    .Build();
            });
        }

        [Test]
        public void ObjectFieldLevelDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<TestObjectWithAddFieldDirectiveByType>()
                .Build();

            var type = server.Schema.KnownTypes.FindDirective("AddField");
            Assert.IsNotNull(type);
            Assert.AreEqual(typeof(AddFieldDirective), type.ObjectType);

            type = server.Schema.KnownTypes.FindDirective(typeof(AddFieldDirective));
            Assert.IsNotNull(type);
            Assert.AreEqual(typeof(AddFieldDirective), type.ObjectType);
        }

        [Test]
        public void ObjectLevelDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<MarkedObject>()
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(ObjectMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void InputObjectLevelDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<InputObjectDirectiveTestController>() // controller has a field with an input object that declares the directive
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(InputObjectMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void InputObjectFieldLevelDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<InputObjectFieldDirectiveTestController>() // controller has a field with an input object that declares the directive
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(InputObjectFieldMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void ArgumentLevelDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<ArgumentMarkedDirectiveTestController>() // input arg on controller method has a directive assigned
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(ArgumentMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void EnumDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<MarkedEnum>() // input arg on controller method has a directive assigned
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(EnumMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void EnumValueDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<MarkedEnumValue>() // input arg on controller method has a directive assigned
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(EnumValueMarkerDirective));
            Assert.IsNotNull(type);
        }
    }
}