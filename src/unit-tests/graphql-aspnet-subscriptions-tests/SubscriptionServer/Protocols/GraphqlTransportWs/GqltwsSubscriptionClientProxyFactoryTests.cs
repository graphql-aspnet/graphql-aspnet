// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlTransportWs
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GqltwsSubscriptionClientProxyFactoryTests
    {
        [Test]
        public void ProtocolAssignmentCheck()
        {
            var factory = new GqltwsSubscriptionClientProxyFactory();
            Assert.AreEqual(GqltwsConstants.PROTOCOL_NAME, factory.Protocol);
        }

        [Test]
        public async Task InstantiationPropertyCheck()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton(new GraphSchema());
            collection.AddSingleton(new SubscriptionServerOptions<GraphSchema>());
            collection.AddSingleton(new Mock<IGraphEventLogger>().Object);
            collection.AddSingleton(new Mock<ISubscriptionEventRouter>().Object);
            collection.AddSingleton(new Mock<IQueryResponseWriter<GraphSchema>>().Object);

            var connect = new Mock<IClientConnection>();
            connect.Setup(x => x.ServiceProvider).Returns(collection.BuildServiceProvider());

            var factory = new GqltwsSubscriptionClientProxyFactory();

            var instance = await factory.CreateClient<GraphSchema>(connect.Object);

            Assert.IsNotNull(instance);
            Assert.AreEqual(factory.Protocol, instance.Protocol);
        }
    }
}