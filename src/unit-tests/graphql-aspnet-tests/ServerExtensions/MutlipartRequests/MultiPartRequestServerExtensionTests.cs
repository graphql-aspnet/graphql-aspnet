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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Exceptions;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Schema;
    using GraphQL.AspNet.Tests.Execution.TestData.DirectiveProcessorTypeSystemLocationTestData;
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
            using var restorePoint = new GraphQLGlobalRestorePoint();
            GraphQLProviders.ScalarProvider = new DefaultScalarGraphTypeProvider();

            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);

            options.RegisterExtension<MultipartRequestServerExtension>();

            Assert.AreEqual(typeof(MultipartRequestGraphQLHttpProcessor<GraphSchema>), options.QueryHandler.HttpProcessorType);
        }

        [Test]
        public void DefaultUsage_ScalarIsRegistered()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
            GraphQLProviders.ScalarProvider = new DefaultScalarGraphTypeProvider();

            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);

            Assert.IsFalse(GraphQLProviders.ScalarProvider.IsScalar(typeof(FileUpload)));

            options.RegisterExtension<MultipartRequestServerExtension>();

            Assert.IsTrue(GraphQLProviders.ScalarProvider.IsScalar(typeof(FileUpload)));
        }

        [Test]
        public void DefaultUsage_WhenProcessorIsAlreadyRegistered_ThrowsException()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
            GraphQLProviders.ScalarProvider = new DefaultScalarGraphTypeProvider();

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
            using var restorePoint = new GraphQLGlobalRestorePoint();
            GraphQLProviders.ScalarProvider = new DefaultScalarGraphTypeProvider();

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
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

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
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

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
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

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
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

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
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

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
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

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