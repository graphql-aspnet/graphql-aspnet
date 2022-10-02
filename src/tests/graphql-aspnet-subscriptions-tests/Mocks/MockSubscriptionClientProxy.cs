// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Moq;

    /// <summary>
    /// A fake subscription client used to test server operations against
    /// connected clients without a real, underlying connection being present.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this proxy exists for.</typeparam>
    public class MockSubscriptionClientProxy<TSchema> : ISubscriptionClientProxy<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockSubscriptionClientProxy{TSchema}" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="securityContext">The security context.</param>
        /// <param name="connectionState">State of the connection.</param>
        public MockSubscriptionClientProxy(
            IServiceProvider serviceProvider,
            IUserSecurityContext securityContext,
            ClientConnectionState connectionState)
        {
            var connection = new Mock<IClientConnection>();
            connection.Setup(x => x.ServiceProvider).Returns(serviceProvider);
            connection.Setup(x => x.SecurityContext).Returns(securityContext);
            connection.Setup(x => x.State).Returns(connectionState);

            this.ClientConnection = connection.Object;

            this.Id = Guid.NewGuid().ToString();
            this.ReceivedEvents = new List<SubscriptionEvent>();
            this.SentMessages = new List<object>();
        }

        /// <inheritdoc />
        public Task CloseConnection(ConnectionCloseStatus reason, string message = null, CancellationToken cancelToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ReceiveEvent(SubscriptionEvent eventData)
        {
            this.ReceivedEvents.Add(eventData);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StartConnection(TimeSpan? keepAliveInterval = null, TimeSpan? initializationTimeout = null, CancellationToken cancelToken = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>
        /// Gets the events this client recieved from the server that it would
        /// have processed.
        /// </summary>
        /// <value>The received events.</value>
        public List<SubscriptionEvent> ReceivedEvents { get; }

        /// <summary>
        /// Gets the messages that this client was to have sent down the
        /// pipe to its underlying connection.
        /// </summary>
        /// <value>The sent messages.</value>
        public List<object> SentMessages { get; }

        /// <summary>
        /// Gets the subscriptions currently registered to this instance.
        /// </summary>
        /// <value>The subscriptions.</value>
        public IEnumerable<ISubscription> Subscriptions => Enumerable.Empty<ISubscription<TSchema>>();

        /// <inheritdoc />
        public string Protocol => "fake-protocol";

        /// <summary>
        /// Gets the underlying client connection so its available to tests. This is a
        /// mock interface, not a real connection.
        /// </summary>
        /// <value>The client connection.</value>
        public IClientConnection ClientConnection { get; }
    }
}