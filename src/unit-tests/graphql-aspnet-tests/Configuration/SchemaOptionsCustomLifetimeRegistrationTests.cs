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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Configuration.SchemaOptionsTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaOptionsCustomLifetimeRegistrationTests
    {
        [Test]
        public void DirectiveAddedWithNonDefaultLifeTime_IsRegisteredAsSuch()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();

            GraphQLProviders
                .GlobalConfiguration
                .ControllerServiceLifeTime = ServiceLifetime.Transient;

            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);

            options.AddDirective<CountableLateBoundDirective>(ServiceLifetime.Singleton);
            options.FinalizeServiceRegistration();

            var descriptor = collection[0];
            Assert.AreEqual(typeof(CountableLateBoundDirective), descriptor.ImplementationType);
            Assert.AreEqual(typeof(CountableLateBoundDirective), descriptor.ServiceType);
            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Test]
        public void ControllerAddedWithNonDefaultLifeTime_IsRegisteredAsSuch()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();

            GraphQLProviders
                .GlobalConfiguration
                .ControllerServiceLifeTime = ServiceLifetime.Transient;

            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);

            options.AddController<MutationAndQueryController>(ServiceLifetime.Singleton);
            options.FinalizeServiceRegistration();

            var descriptor = collection[0];
            Assert.AreEqual(typeof(MutationAndQueryController), descriptor.ImplementationType);
            Assert.AreEqual(typeof(MutationAndQueryController), descriptor.ServiceType);
            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
        }
    }
}