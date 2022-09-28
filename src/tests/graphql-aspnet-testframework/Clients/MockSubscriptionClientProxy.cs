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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;
    using Moq;

    /// <summary>
    /// A fake subscription client used to test server operations against
    /// connected clients without a specific protocol being in the mix.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this proxy exists for.</typeparam>
    public class MockSubscriptionClientProxy<TSchema> : ISubscriptionClientProxy<TSchema>
        where TSchema : class, ISchema
    {
        /// <inheritdoc />
        public event EventHandler ConnectionOpening;

        /// <inheritdoc />
        public event EventHandler ConnectionClosed;

        /// <inheritdoc />
        public event EventHandler ConnectionClosing;

        /// <inheritdoc />
        public event EventHandler<SubscriptionFieldEventArgs> SubscriptionRouteAdded;

        /// <inheritdoc />
        public event EventHandler<SubscriptionFieldEventArgs> SubscriptionRouteRemoved;

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

            this.ClientConnection = connection.Object;

            this.Id = Guid.NewGuid().ToString();
            this.ReceivedEvents = new List<(SchemaItemPath FieldPath, object SourceData)>();
            this.SentMessages = new List<object>();
            this.State = connectionState;
        }

        /// <inheritdoc />
        public Task CloseConnection(ConnectionCloseStatus reason, string message = null, CancellationToken cancelToken = default)
        {
            this.ConnectionClosing?.Invoke(this, new EventArgs());
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ReceiveEvent(SchemaItemPath field, object sourceData, CancellationToken cancelToken = default)
        {
            this.ReceivedEvents.Add((field, sourceData));
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SendErrorMessage(IGraphMessage graphMessage, string subscriptionId = null)
        {
            Validation.ThrowIfNull(graphMessage, nameof(graphMessage));
            this.SentMessages.Add(graphMessage);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StartConnection(TimeSpan? keepAliveInterval = null, TimeSpan? initializationTimeout = null)
        {
            this.ConnectionOpening?.Invoke(this, new EventArgs());
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public ClientConnectionState State { get; }

        /// <summary>
        /// Gets the events this client recieved from the server that it would
        /// have processed.
        /// </summary>
        /// <value>The received events.</value>
        public List<(SchemaItemPath FieldPath, object SourceData)> ReceivedEvents { get; }

        /// <summary>
        /// Gets the messages that this client was to have sent down the
        /// pipe to its underlying connection.
        /// </summary>
        /// <value>The sent messages.</value>
        public List<object> SentMessages { get; }

        /// <inheritdoc />
        public IEnumerable<ISubscription> Subscriptions => Enumerable.Empty<ISubscription<TSchema>>();

        /// <inheritdoc />
        public string Protocol => "fake-protocol";

        /// <inheritdoc />
        public IClientConnection ClientConnection { get; }
    }
}