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
    using GraphQL.AspNet.Configuration.Exceptions;
    using GraphQL.AspNet.Tests.Execution.TestData.TypeSystemDirectiveTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class TypeSystemDirectiveDiscoveryTests
    {
        [Test]
        public void DirectiveDeclaredByName_WhenNotExplicitlyIncluded_ThrowsStartupException()
        {
            Assert.Throws<SchemaConfigurationException>(() =>
            {
                var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                    .AddType<TestPersonWithResolverExtensionDirectiveByName>()
                    .Build();
            });
        }

        [Test]
        public void ObjectFieldLevelDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<TestObjectWithAddFieldDirectiveByType>()
                .Build();

            var type = server.Schema.KnownTypes.FindDirective("AddField");
            Assert.IsNotNull(type);
            Assert.AreEqual(typeof(AddFieldDirective), type.ObjectType);

            type = server.Schema.KnownTypes.FindDirective(typeof(AddFieldDirective));
            Assert.IsNotNull(type);
            Assert.AreEqual(typeof(AddFieldDirective), type.ObjectType);
        }

        [Test]
        public void InputObjectLevelDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<InputObjectDirectiveTestController>() // controller has a field with an input object that declares the directive
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(InputObjectMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void InputObjectFieldLevelDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<InputObjectFieldDirectiveTestController>() // controller has a field with an input object that declares the directive
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(InputObjectFieldMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void ArgumentDefinitionLevelDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<ArgumentMarkedDirectiveTestController>() // input arg on controller method has a directive assigned
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(ArgumentMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void EnumDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<MarkedEnum>() // input arg on controller method has a directive assigned
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(EnumMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void EnumValueDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<MarkedEnumValue>() // input arg on controller method has a directive assigned
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(EnumValueMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void InterfaceDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<IMarkedInterface>()
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(InterfaceMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void UnionDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<MarkedUnion>()
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(UnionMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void ScalarDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();

            GraphQLProviders.ScalarProvider.RegisterCustomScalar(typeof(MarkedScalarByAttribute));

            // the object has a property that returns the custom scalar
            // forcing the enclusion of the scalar and thus the directive on said scalar
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<ObjectWithMarkedScalar>()
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(ScalarMarkerDirective));
            Assert.IsNotNull(type);
        }

        [Test]
        public void SchemaDirective_DeclaredByType_WhenNotExplicitlyIncluded_IsLocatedAndIncluded()
        {
            var server = new TestServerBuilder<MarkedSchema>(TestOptions.UseCodeDeclaredNames)
                .Build();

            var type = server.Schema.KnownTypes.FindDirective(typeof(SchemaMarkerDirective));
            Assert.IsNotNull(type);
        }
    }
}