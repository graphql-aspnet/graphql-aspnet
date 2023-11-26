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
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    public class MultipartRequestExtensionMethodsTests
    {
        public class SecondSchema : GraphSchema
        {
        }

        [Test]
        public void AddMultipartRequestSupport_DoesNotThrowExceptionWhenNotPassingAction()
        {
            var options = new SchemaOptions<GraphSchema>(new ServiceCollection());
            options.AddMultipartRequestSupport();
        }

        [Test]
        public void AddMultipartRequestSupport_ProvidedActionMethodIsCalled()
        {
            var wasCalled = false;

            var collection = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(collection);
            options.AddMultipartRequestSupport(o =>
            {
                wasCalled = true;
            });

            Assert.AreEqual(3, collection.Count);

            Assert.IsNotNull(collection
                .SingleOrDefault(x =>
                    x.ServiceType == typeof(IMultipartRequestConfiguration<GraphSchema>)));

            Assert.IsNotNull(collection
                .SingleOrDefault(x =>
                    x.ServiceType == typeof(IMultiPartHttpFormPayloadParser<GraphSchema>)));

            Assert.IsNotNull(collection
                .SingleOrDefault(x =>
                    x.ServiceType == typeof(IFileUploadScalarValueMaker)));

            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void AddMultipartRequestSupport_OnMultipleSchemas_RegistersGlobalEntitiesOnce()
        {
            var wasCalled1 = false;
            var collection = new ServiceCollection();

            var options1 = new SchemaOptions<GraphSchema>(collection);
            options1.AddMultipartRequestSupport(o =>
            {
                wasCalled1 = true;
            });

            Assert.IsTrue(wasCalled1);

            var wasCalled2 = false;
            var options2 = new SchemaOptions<SecondSchema>(collection);
            options2.AddMultipartRequestSupport(o =>
            {
                wasCalled2 = true;
            });

            Assert.AreEqual(5, collection.Count);

            Assert.IsNotNull(collection
                .SingleOrDefault(x =>
                    x.ServiceType == typeof(IMultipartRequestConfiguration<GraphSchema>)));

            Assert.IsNotNull(collection
                .SingleOrDefault(x =>
                    x.ServiceType == typeof(IMultiPartHttpFormPayloadParser<GraphSchema>)));

            Assert.IsNotNull(collection
                .SingleOrDefault(x =>
                    x.ServiceType == typeof(IMultipartRequestConfiguration<SecondSchema>)));

            Assert.IsNotNull(collection
                .SingleOrDefault(x =>
                    x.ServiceType == typeof(IMultiPartHttpFormPayloadParser<SecondSchema>)));

            Assert.IsNotNull(collection
                .SingleOrDefault(x =>
                    x.ServiceType == typeof(IFileUploadScalarValueMaker)));

            Assert.IsTrue(wasCalled2);
        }
    }
}