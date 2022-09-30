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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.Defaults.TestData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.Options;
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

            var connection = new Mock<IClientConnection>();
            connection.Setup(x => x.SecurityContext).Returns(securityContext.Object);

            var client = new Mock<ISubscriptionClientProxy<GraphSchema>>();

            client.Setup(x => x.SecurityContext).Returns(securityContext.Object);

            var result = await server.RegisterNewClient(client.Object);

            Assert.IsFalse(result);

            client.Verify(
                x => x.CloseConnection(
                    It.IsAny<ConnectionCloseStatus>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Test]
        public async Task ReceiveEvent_WithNoClients_YieldsNothing()
        {
            var router = new Mock<ISubscriptionEventRouter>();

            var options = new SubscriptionServerOptions<GraphSchema>();

            var server = new DefaultSubscriptionServer<GraphSchema>(
                new GraphSchema(),
                options,
                router.Object);

            var evt = new SubscriptionEvent()
            {
                Data = null,
                DataTypeName = typeof(TwoPropertyObject).FullName,
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "fakeEvent",
            };

            var count = await server.ReceiveEvent(evt);
            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ReceiveEvent_WithNoData_YieldsNothing()
        {
            var router = new Mock<ISubscriptionEventRouter>();

            var options = new SubscriptionServerOptions<GraphSchema>();

            var server = new DefaultSubscriptionServer<GraphSchema>(
                new GraphSchema(),
                options,
                router.Object);

            var count = await server.ReceiveEvent(null);
            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task WhenTheServerReceivesAnEvent_WithARegisteredClient_ClientRecievesEvent()
        {
            var graphqlServer = new TestServerBuilder()
                .AddGraphController<SubscriptionTestController>()
                .AddSubscriptionPublishing()
                .AddSubscriptionServer()
                .Build();

            var router = new Mock<ISubscriptionEventRouter>();
            var options = new SubscriptionServerOptions<GraphSchema>();
            var server = new DefaultSubscriptionServer<GraphSchema>(
                graphqlServer.Schema,
                options,
                router.Object);

            var type = graphqlServer.Schema.KnownTypes.FindGraphType("Subscription_SubscriptionTest") as IObjectGraphType;
            var field = type.Fields.FindField("doSub") as ISubscriptionGraphField;

            // register a fake client that "subscribes" to the event
            var client = new Mock<ISubscriptionClientProxy<GraphSchema>>();
            var connection = new Mock<IClientConnection>();
            var securityContext = new Mock<IUserSecurityContext>();
            connection.Setup(x => x.SecurityContext).Returns(securityContext.Object);
            client.Setup(x => x.SecurityContext).Returns(securityContext.Object);

            await server.RegisterNewClient(client.Object);

            client.Raise(x => x.SubscriptionRouteAdded += null, new SubscriptionFieldEventArgs(field));

            var evt = new SubscriptionEvent()
            {
                Data = new TwoPropertyObject(),
                DataTypeName = SchemaExtensions.RetrieveFullyQualifiedTypeName(typeof(TwoPropertyObject)),
                SchemaTypeName = SchemaExtensions.RetrieveFullyQualifiedTypeName(typeof(GraphSchema)),
                EventName = field.Route.ToString(),
            };

            // mimic new data available for that subscription
            var count = await server.ReceiveEvent(evt);

            Assert.AreEqual(1, count);

            client.Verify(
                x => x.ReceiveEvent(
                    It.IsAny<SchemaItemPath>(),
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }
    }
}