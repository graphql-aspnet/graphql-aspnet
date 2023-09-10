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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration.Startup;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Configuration.SchemaInjectorTestData;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaInjectorTests
    {
        [TestCase(typeof(ObjectWithInvalidMethodParam1))]
        [TestCase(typeof(ObjectWithInvalidMethodParam2))]
        [TestCase(typeof(ObjectWithInvalidMethodParam3))]
        [TestCase(typeof(ObjectWithInvalidMethodParam4))]
        [TestCase(typeof(ObjectWithInvalidMethodParam5))]
        [TestCase(typeof(ObjectWithInvalidMethodParam6))]
        [TestCase(typeof(ObjectWithInvalidPropertyType))]
        public void UseGraphQL_WithObjectWithInvalidMethod_DoesNotFail(Type objectType)
        {
            var collection = new ServiceCollection();

            var injector = GraphQLSchemaInjectorFactory.Create<GraphSchema>(
                collection,
                o =>
                {
                    o.AddType(objectType);
                });

            injector.ConfigureServices();

            var provider = collection.BuildServiceProvider();

            // no exception should be thrown
            injector.UseSchema(provider);
        }
    }
}