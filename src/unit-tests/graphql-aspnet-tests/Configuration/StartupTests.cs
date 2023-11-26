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
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Schemas;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class StartupTests
    {
        public class Schema2 : GraphSchema
        {
        }

        [Test]
        public void AttemptingToAddGraphQL_ForDifferentSchemas_isFIne()
        {
            var collection = new ServiceCollection();
            collection.AddGraphQL<GraphSchema>();
            collection.AddGraphQL<Schema2>();
        }

        [Test]
        public void AttemptingToAddGraphQL_TwiceForSameSchema_ThrowsException()
        {
            var collection = new ServiceCollection();
            collection.AddGraphQL<GraphSchema>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                collection.AddGraphQL<GraphSchema>();
            });
        }

        [Test]
        public void AttemptingToAddGraphQL_ForDifferentSchemas_YieldsTwoInjectorsInPRovider()
        {
            var collection = new ServiceCollection();
            collection.AddGraphQL<GraphSchema>();
            collection.AddGraphQL<Schema2>();

            var provider = collection.BuildServiceProvider();

            var services = provider.GetServices<ISchemaInjector>();
            Assert.AreEqual(2, services.Count());
        }
    }
}