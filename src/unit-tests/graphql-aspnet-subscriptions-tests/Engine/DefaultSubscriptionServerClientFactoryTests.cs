// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer.Exceptions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultSubscriptionServerClientFactoryTests
    {
        public class FakeClientFactory : ISubscriptionClientProxyFactory
        {
            public FakeClientFactory(string protocol)
            {
                this.Protocol = protocol;
            }

            public Task<ISubscriptionClientProxy<TSchema>> CreateClient<TSchema>(IClientConnection connection)
                where TSchema : class, ISchema
            {
                var proxy = new Mock<ISubscriptionClientProxy<TSchema>>();
                proxy.Setup(x => x.Protocol).Returns(this.Protocol);
                return Task.FromResult(proxy.Object);
            }

            public string Protocol { get; }
        }

        public IClientConnection CreateConnection(
            string requestedProtocols,
            SubscriptionServerOptions<GraphSchema> schemaOptions)
        {
            var collection = new ServiceCollection();
            collection.AddSingleton(schemaOptions);

            var connection = new Mock<IClientConnection>();
            connection.Setup(x => x.RequestedProtocols).Returns(requestedProtocols);
            connection.Setup(x => x.ServiceProvider).Returns(collection.BuildServiceProvider());

            return connection.Object;
        }

        [Test]
        public void NoRegisteredClientFactories_ThrowsException()
        {
            var clientFactories = new List<ISubscriptionClientProxyFactory>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var service = new DefaultSubscriptionServerClientFactory(clientFactories);
            });
        }

        [Test]
        public async Task SupportedSingleProtocol_ReturnsClientInstance()
        {
            var clientFactories = new List<ISubscriptionClientProxyFactory>();
            clientFactories.Add(new FakeClientFactory("protocol1"));
            clientFactories.Add(new FakeClientFactory("protocol2"));

            var subOptions = new SubscriptionServerOptions<GraphSchema>();

            var connection = this.CreateConnection("protocol1", subOptions);

            var service = new DefaultSubscriptionServerClientFactory(clientFactories);

            var client = await service.CreateSubscriptionClientAsync<GraphSchema>(connection);

            Assert.IsNotNull(client);
            Assert.AreEqual("protocol1", client.Protocol);
        }

        [Test]
        public void UnknownSingleProtocol_ThrowsException()
        {
            var clientFactories = new List<ISubscriptionClientProxyFactory>();
            clientFactories.Add(new FakeClientFactory("protocol1"));
            clientFactories.Add(new FakeClientFactory("protocol2"));

            var subOptions = new SubscriptionServerOptions<GraphSchema>();

            var service = new DefaultSubscriptionServerClientFactory(clientFactories);

            var connection = this.CreateConnection("unknown-protocol", subOptions);

            var exception = Assert.ThrowsAsync<UnsupportedClientProtocolException>(async () =>
            {
                var client = await service.CreateSubscriptionClientAsync<GraphSchema>(connection);
            });

            Assert.AreEqual("unknown-protocol", exception.Protocol);
        }

        [Test]
        public void UnsupportedSingleProtocol_ThrowsException()
        {
            var clientFactories = new List<ISubscriptionClientProxyFactory>();
            clientFactories.Add(new FakeClientFactory("protocol1"));
            clientFactories.Add(new FakeClientFactory("protocol2"));

            var subOptions = new SubscriptionServerOptions<GraphSchema>();
            subOptions.SupportedMessageProtocols = new HashSet<string>() { "protocol1" };

            var service = new DefaultSubscriptionServerClientFactory(clientFactories);

            var connection = this.CreateConnection("protocol2", subOptions);

            var exception = Assert.ThrowsAsync<UnsupportedClientProtocolException>(async () =>
            {
                var client = await service.CreateSubscriptionClientAsync<GraphSchema>(connection);
            });

            Assert.AreEqual("protocol2", exception.Protocol);
        }

        [Test]
        public async Task MultipleRequestedProtocols_FirstMatchIsCreated()
        {
            var clientFactories = new List<ISubscriptionClientProxyFactory>();
            clientFactories.Add(new FakeClientFactory("protocol1"));
            clientFactories.Add(new FakeClientFactory("protocol2"));

            var subOptions = new SubscriptionServerOptions<GraphSchema>();

            var service = new DefaultSubscriptionServerClientFactory(clientFactories);

            var connection = this.CreateConnection("protocol3, protocol1, protocol2", subOptions);
            var client = await service.CreateSubscriptionClientAsync<GraphSchema>(connection);

            Assert.IsNotNull(client);
            Assert.AreEqual("protocol1", client.Protocol);
        }

        [Test]
        public void MultipleUnknownProtocols_ThrowsException()
        {
            var clientFactories = new List<ISubscriptionClientProxyFactory>();
            clientFactories.Add(new FakeClientFactory("protocol1"));
            clientFactories.Add(new FakeClientFactory("protocol2"));

            var subOptions = new SubscriptionServerOptions<GraphSchema>();

            var service = new DefaultSubscriptionServerClientFactory(clientFactories);

            var connection = this.CreateConnection("protocol3, protocol4", subOptions);

            var exception = Assert.ThrowsAsync<UnsupportedClientProtocolException>(async () =>
            {
                var client = await service.CreateSubscriptionClientAsync<GraphSchema>(connection);
            });

            Assert.AreEqual("protocol3, protocol4", exception.Protocol);
        }

        [Test]
        public void MultipleUnSupportedProtocols_ThrowsException()
        {
            var clientFactories = new List<ISubscriptionClientProxyFactory>();
            clientFactories.Add(new FakeClientFactory("protocol1"));
            clientFactories.Add(new FakeClientFactory("protocol2"));
            clientFactories.Add(new FakeClientFactory("protocol3"));
            clientFactories.Add(new FakeClientFactory("protocol4"));

            var subOptions = new SubscriptionServerOptions<GraphSchema>();
            subOptions.SupportedMessageProtocols = new HashSet<string>() { "protocol1", "protocol2" };

            var service = new DefaultSubscriptionServerClientFactory(clientFactories);

            var connection = this.CreateConnection(
                "protocol3, protocol4",
                subOptions);

            var exception = Assert.ThrowsAsync<UnsupportedClientProtocolException>(async () =>
            {
                var client = await service.CreateSubscriptionClientAsync<GraphSchema>(connection);
            });

            Assert.AreEqual("protocol3, protocol4", exception.Protocol);
        }

        [Test]
        public async Task NoSuppliedProtocl_WhenSchemaHasDefault_DefaultIsUsed()
        {
            var clientFactories = new List<ISubscriptionClientProxyFactory>();
            clientFactories.Add(new FakeClientFactory("protocol1"));
            clientFactories.Add(new FakeClientFactory("protocol2"));

            var subOptions = new SubscriptionServerOptions<GraphSchema>();
            subOptions.DefaultMessageProtocol = "protocol1";

            var service = new DefaultSubscriptionServerClientFactory(clientFactories);

            var connection = this.CreateConnection(
                string.Empty,
                subOptions);

            var client = await service.CreateSubscriptionClientAsync<GraphSchema>(connection);

            Assert.AreEqual("protocol1", client.Protocol);
        }

        [Test]
        public void NoSuppliedProtocl_NoSchemaDefault_ThrowsException()
        {
            var clientFactories = new List<ISubscriptionClientProxyFactory>();
            clientFactories.Add(new FakeClientFactory("protocol1"));
            clientFactories.Add(new FakeClientFactory("protocol2"));

            var subOptions = new SubscriptionServerOptions<GraphSchema>();

            var service = new DefaultSubscriptionServerClientFactory(clientFactories);

            var connection = this.CreateConnection(
                string.Empty,
                subOptions);

            var exception = Assert.ThrowsAsync<UnsupportedClientProtocolException>(async () =>
            {
                var client = await service.CreateSubscriptionClientAsync<GraphSchema>(connection);
            });

            Assert.AreEqual("~none~", exception.Protocol);
        }
    }
}