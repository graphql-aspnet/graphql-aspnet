// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Defaults
{
    using System.Runtime.CompilerServices;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultSubscriptionServerTests
    {
        [Test]
        public async Task RegisteringUnAuthenticatedClient_WhenAuthenticationRequired_ClosesConnection()
        {
            var router = new Mock<ISubscriptionEventRouter>();

            var options = new SubscriptionServerOptions<GraphSchema>();
            options.AuthenticatedRequestsOnly = true;

            var server = new DefaultSubscriptionServer<GraphSchema>(
                new GraphSchema(),
                options,
                router.Object);

            var securityContext = new Mock<IUserSecurityContext>();
            securityContext.Setup(x => x.DefaultUser).Returns(null as ClaimsPrincipal);

            var client = new Mock<ISubscriptionClientProxy<GraphSchema>>();
            client.Setup(x => x.SecurityContext).Returns(securityContext.Object);

            var result = await server.RegisterNewClient(client.Object);

            Assert.IsFalse(result);

            client.Verify(x => x.SendErrorMessage(It.IsAny<IGraphMessage>()), Times.Once());
            client.Verify(
                x => x.CloseConnection(
                    It.IsAny<ClientConnectionCloseStatus>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }
    }
}