// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration
{
    using System;
    using GraphQL.AspNet.Configuration.Exceptions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Configuration.ConfigurationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaOptionsAddTypesTests
    {
        [Test]
        public void AddGraphType_AddAsObjectAndInputObject_BothAreAddedToSchema()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphQL(options =>
            {
                options.AddGraphType<TwoPropertyObject>(TypeKind.OBJECT);
                options.AddGraphType<TwoPropertyObject>(TypeKind.INPUT_OBJECT);
            });

            var server = builder.Build();

            var objectType = server.Schema.KnownTypes.FindGraphType(typeof(TwoPropertyObject), TypeKind.OBJECT);
            var inputType = server.Schema.KnownTypes.FindGraphType(typeof(TwoPropertyObject), TypeKind.INPUT_OBJECT);

            Assert.IsNotNull(objectType);
            Assert.IsNotNull(inputType);
        }

        [Test]
        public void AddGraphType_ByType_AddAsObjectAndInputObject_BothAreAddedToSchema()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphQL(options =>
            {
                options.AddGraphType(typeof(TwoPropertyObject), TypeKind.OBJECT);
                options.AddGraphType(typeof(TwoPropertyObject), TypeKind.INPUT_OBJECT);
            });

            var server = builder.Build();

            var objectType = server.Schema.KnownTypes.FindGraphType(typeof(TwoPropertyObject), TypeKind.OBJECT);
            var inputType = server.Schema.KnownTypes.FindGraphType(typeof(TwoPropertyObject), TypeKind.INPUT_OBJECT);

            Assert.IsNotNull(objectType);
            Assert.IsNotNull(inputType);
        }

        [Test]
        public void AddType_AddAsObjectAndInputObject_BothAreAddedToSchema()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphQL(options =>
            {
                options.AddType<TwoPropertyObject>(TypeKind.OBJECT);
                options.AddType<TwoPropertyObject>(TypeKind.INPUT_OBJECT);
            });

            var server = builder.Build();

            var objectType = server.Schema.KnownTypes.FindGraphType(typeof(TwoPropertyObject), TypeKind.OBJECT);
            var inputType = server.Schema.KnownTypes.FindGraphType(typeof(TwoPropertyObject), TypeKind.INPUT_OBJECT);

            Assert.IsNotNull(objectType);
            Assert.IsNotNull(inputType);
        }

        [Test]
        public void AddType_ByType_AddAsObjectAndInputObject_BothAreAddedToSchema()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphQL(options =>
            {
                options.AddType(typeof(TwoPropertyObject), TypeKind.OBJECT);
                options.AddType(typeof(TwoPropertyObject), TypeKind.INPUT_OBJECT);
            });

            var server = builder.Build();

            var objectType = server.Schema.KnownTypes.FindGraphType(typeof(TwoPropertyObject), TypeKind.OBJECT);
            var inputType = server.Schema.KnownTypes.FindGraphType(typeof(TwoPropertyObject), TypeKind.INPUT_OBJECT);

            Assert.IsNotNull(objectType);
            Assert.IsNotNull(inputType);
        }

        [TestCase(typeof(FanController))]
        [TestCase(typeof(Sample1Directive))]
        public void AddGraphType_WhenGivenController_ThrowsException(Type testType)
        {
            bool successfulFailure = false;
            var builder = new TestServerBuilder();
            builder.AddGraphQL(options =>
            {
                var ex = Assert.Throws<SchemaConfigurationException>(() =>
                {
                    options.AddGraphType(testType);
                });

                successfulFailure = ex != null;
            });

            var server = builder.Build();
            if (!successfulFailure)
                Assert.Fail("Expected exception");
        }
    }
}