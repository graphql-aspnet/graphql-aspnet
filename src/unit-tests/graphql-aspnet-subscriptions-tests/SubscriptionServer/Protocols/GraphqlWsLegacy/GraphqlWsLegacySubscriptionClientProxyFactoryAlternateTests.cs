﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlWsLegacy
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy;
    using Microsoft.Extensions.DependencyInjection;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class GraphqlWsLegacySubscriptionClientProxyFactoryAlternateTests
    {
        [Test]
        public void ProtocolAssignmentCheck()
        {
            var factory = new GraphqlWsLegacySubscriptionClientProxyFactoryAlternate();
            Assert.AreEqual(GraphqlWsLegacyConstants.ALTERNATE_PROTOCOL_NAME, factory.Protocol);
        }

        [Test]
        public async Task InstantiationPropertyCheck()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton(new GraphSchema());
            collection.AddSingleton(new SubscriptionServerOptions<GraphSchema>());
            collection.AddSingleton(Substitute.For<IGraphEventLogger>());
            collection.AddSingleton(Substitute.For<ISubscriptionEventRouter>());
            collection.AddSingleton(Substitute.For<IQueryResponseWriter<GraphSchema>>());

            var connect = Substitute.For<IClientConnection>();
            connect.ServiceProvider.Returns(collection.BuildServiceProvider());

            var factory = new GraphqlWsLegacySubscriptionClientProxyFactoryAlternate();

            var instance = await factory.CreateClient<GraphSchema>(connect);

            Assert.IsNotNull(instance);
            Assert.AreEqual(GraphqlWsLegacyConstants.ALTERNATE_PROTOCOL_NAME, instance.Protocol);
        }
    }
}