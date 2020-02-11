// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions.Apollo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A baseline component acts to centralize the subscription server operations, regardless of if
    /// this server is in-process with the primary graphql runtime or out-of-process on a seperate instance.
    /// </summary>
    /// <typeparam name="TSchema">The schema type this server is registered to handle.</typeparam>
    public class ApolloSubscriptionServer<TSchema> : ISubscriptionServer<TSchema>
        where TSchema : class, ISchema
    {
        private ApolloClientSupervisor<TSchema> _supervisor;
        private Dictionary<string, string> _eventMap;
        private readonly TSchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionServer{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema this server will function with.</param>
        /// <param name="supervisor">The supervisor in charge of managing client connections for a
        /// given schema.</param>
        public ApolloSubscriptionServer(TSchema schema, ApolloClientSupervisor<TSchema> supervisor)
        {
            _supervisor = Validation.ThrowIfNullOrReturn(supervisor, nameof(supervisor));
            _eventMap = new Dictionary<string, string>();
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));

            this.PopulateEventMap();
        }

        /// <summary>
        /// Populates the event map to map any friendly secondary event names to their primary route paths.
        /// </summary>
        private void PopulateEventMap()
        {
            var subscriptionOperation = _schema.KnownTypes.SingleOrDefault(x => x is IGraphOperation && ((IGraphOperation)x).OperationType == GraphCollection.Subscription) as IGraphOperation;

            // TODO: need some validation to prevent duplicate event names for subscriptions
            if (subscriptionOperation != null)
            {
                foreach (var field in subscriptionOperation.Fields.OfType<ISubscriptionGraphField>())
                {
                    _eventMap.Add(field.Route.Path, field.Route.Path);
                    if (!string.IsNullOrWhiteSpace(field.EventName))
                        _eventMap.Add(field.EventName, field.Route.Path);
                }
            }
        }

        /// <summary>
        /// Receives the event (packaged and published by the proxy) and performs
        /// the required work to send it to connected clients.
        /// </summary>
        /// <typeparam name="TData">The type of the data being recieved on the event.</typeparam>
        /// <param name="subscriptionEvent">A subscription event.</param>
        /// <returns>Task.</returns>
        public Task PublishEvent<TData>(SubscriptionEvent<TData> subscriptionEvent)
        {
            if (subscriptionEvent != null && _eventMap.ContainsKey(subscriptionEvent.EventName))
            {
                // normalize the event name by its full path
                var eventName = _eventMap[subscriptionEvent.EventName];

                // fetch all subscriptions that need to be altered
                var subscriptions = _supervisor.RetrieveSubscriptions(eventName);
            }

            return Task.CompletedTask;
        }
    }
}