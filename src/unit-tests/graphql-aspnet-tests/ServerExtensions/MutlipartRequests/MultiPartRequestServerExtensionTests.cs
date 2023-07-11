// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ServerExtensions.MutlipartRequests
{
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Exceptions;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Schema;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.ServerExtensions.MutlipartRequests.TestData;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class MultiPartRequestServerExtensionTests
    {
        [Test]
        public void DefaultUsage_DefaultProcessorIsRegistered()
        {
            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);

            options.RegisterExtension<MultipartRequestServerExtension>();

            Assert.AreEqual(typeof(MultipartRequestGraphQLHttpProcessor<GraphSchema>), options.QueryHandler.HttpProcessorType);
        }

        [Test]
        public void DefaultUsage_ScalarIsRegistered()
        {
            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);

            options.RegisterExtension<MultipartRequestServerExtension>();

            Assert.IsTrue(options.SchemaTypesToRegister.Any(x => x.Type == typeof(FileUploadScalarGraphType)));
        }

        [Test]
        public void DefaultUsage_WhenProcessorIsAlreadyRegistered_ThrowsException()
        {
            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);
            options.QueryHandler.HttpProcessorType = typeof(DefaultGraphQLHttpProcessor<GraphSchema>);

            Assert.Throws<SchemaConfigurationException>(() =>
            {
                options.RegisterExtension<MultipartRequestServerExtension>();
            });
        }

        [Test]
        public void DefaultUsage_CustomProcessorIsChangedToSomethingNotCompatiable_ThrowsExceptionOnUsage()
        {
            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);

            var extension = new MultipartRequestServerExtension();

            options.RegisterExtension(extension);
            options.QueryHandler.HttpProcessorType = typeof(DefaultGraphQLHttpProcessor<GraphSchema>);

            var provider = collection.BuildServiceProvider();
            var scopedProvider = provider.CreateScope();

            Assert.Throws<SchemaConfigurationException>(() =>
            {
                extension.UseExtension(serviceProvider: scopedProvider.ServiceProvider);
            });
        }

        [Test]
        public void DeclineDefaultProcessor_CustomProcessorIsSetManually_NoException()
        {
            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);

            var extension = new MultipartRequestServerExtension((o) =>
            {
                o.RegisterMultipartRequestHttpProcessor = false;
                o.RequireMultipartRequestHttpProcessor = false;
            });

            options.RegisterExtension(extension);
            options.QueryHandler.HttpProcessorType = typeof(DefaultGraphQLHttpProcessor<GraphSchema>);

            var provider = collection.BuildServiceProvider();
            var scopedProvider = provider.CreateScope();

            extension.UseExtension(serviceProvider: scopedProvider.ServiceProvider);
        }

        [Test]
        public void UseExtension_WithNoProvider_DoesNothing()
        {
            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);

            var extension = new MultipartRequestServerExtension((o) =>
            {
                o.RegisterMultipartRequestHttpProcessor = false;
                o.RequireMultipartRequestHttpProcessor = false;
            });

            options.RegisterExtension(extension);

            // no provider passed
            var provider = collection.BuildServiceProvider();
            var scopedProvider = provider.CreateScope();

            extension.UseExtension(serviceProvider: scopedProvider.ServiceProvider);
        }

        [Test]
        public void UseExtension_CheckingForACompatiableProvider_NoConfigChanges_ProviderFirst_ThrowsException()
        {
            var extension = new MultipartRequestServerExtension((o) => { });
            var serverBuilder = new TestServerBuilder();

            serverBuilder.TryAddSingleton<IGraphQLHttpProcessor<GraphSchema>, CompatiableMultipartProcessor<GraphSchema>>();
            serverBuilder.AddGraphQL(options =>
                {
                    // compatiable provider placed before extension invoked
                    options.QueryHandler.HttpProcessorType = typeof(CompatiableMultipartProcessor<GraphSchema>);

                    options.RegisterExtension(extension);
                });

            Assert.Throws<SchemaConfigurationException>(() =>
            {
                var server = serverBuilder.Build();
            });
        }

        [Test]
        public void UseExtension_CheckingForACompatiableProvider_NoConfigChanges_ProviderLast_IsValid()
        {
            var extension = new MultipartRequestServerExtension((o) => { });
            var serverBuilder = new TestServerBuilder();

            serverBuilder.TryAddSingleton<IGraphQLHttpProcessor<GraphSchema>, CompatiableMultipartProcessor<GraphSchema>>();
            serverBuilder.AddGraphQL(options =>
            {
                options.RegisterExtension(extension);

                // compatiable provider placed after extension invoked
                options.QueryHandler.HttpProcessorType = typeof(CompatiableMultipartProcessor<GraphSchema>);
            });

            var server = serverBuilder.Build();
        }

        [Test]
        public void UseExtension_NotRegisteringProvider_CheckingForACompatiableProvider_ProviderLast_IsValid()
        {
            var extension = new MultipartRequestServerExtension((o) =>
            {
                o.RegisterMultipartRequestHttpProcessor = false;
                o.RequireMultipartRequestHttpProcessor = true;
            });

            var serverBuilder = new TestServerBuilder();

            serverBuilder.TryAddSingleton<IGraphQLHttpProcessor<GraphSchema>, CompatiableMultipartProcessor<GraphSchema>>();
            serverBuilder.AddGraphQL(options =>
            {
                options.RegisterExtension(extension);

                // compatiable provider placed after extension invoked
                options.QueryHandler.HttpProcessorType = typeof(CompatiableMultipartProcessor<GraphSchema>);
            });

            var server = serverBuilder.Build();
        }

        [Test]
        public void UseExtension_NotRegisteringProvider_CheckingForACompatiableProvider_ProviderFirst_IsValid()
        {
            var extension = new MultipartRequestServerExtension((o) =>
            {
                o.RegisterMultipartRequestHttpProcessor = false;
                o.RequireMultipartRequestHttpProcessor = true;
            });

            var serverBuilder = new TestServerBuilder();

            serverBuilder.TryAddSingleton<IGraphQLHttpProcessor<GraphSchema>, CompatiableMultipartProcessor<GraphSchema>>();
            serverBuilder.AddGraphQL(options =>
            {
                // compatiable provider placed after extension invoked
                options.QueryHandler.HttpProcessorType = typeof(CompatiableMultipartProcessor<GraphSchema>);

                options.RegisterExtension(extension);
            });

            var server = serverBuilder.Build();
        }
    }
}