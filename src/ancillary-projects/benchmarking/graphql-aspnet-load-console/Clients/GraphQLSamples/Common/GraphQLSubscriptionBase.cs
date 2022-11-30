// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.GraphQLSamples.Common
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common;
    using GraphQL.Client.Http;

    using GQLRequest = GraphQLRequest;

    /// <summary>
    /// A base client script for executing and monitoring a graphql subscription.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned from the received
    /// subscription events.</typeparam>
    public abstract class GraphQLSubscriptionBase<TResult> : GraphQLScriptBase
    {
        private static int _clientNumber = 1;

        private bool _isDisposed;

        private Dictionary<Subscription<TResult>, TaskCompletionSource> _subscriptions;
        private GraphQLHttpClient _client;
        private int _iterationsInitiated;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLSubscriptionBase{TResult}" /> class.
        /// </summary>
        /// <param name="url">The url to send graphql requests to.</param>
        /// <param name="scriptNumber">The overall script number assigned to this instance.</param>
        /// <param name="executionCategory">The execution catgory this script falls under.</param>
        /// <param name="categorySuffix">The category suffix to apply to this individual script.</param>
        public GraphQLSubscriptionBase(
            string url,
            int scriptNumber,
            string executionCategory,
            string categorySuffix = "")
            : base(url, scriptNumber, executionCategory, categorySuffix)
        {
            this.ClientNumber = _clientNumber++;
            _subscriptions = new Dictionary<Subscription<TResult>, TaskCompletionSource>();
        }

        /// <summary>
        /// Creates a new request to start a subscription.
        /// </summary>
        /// <returns>GraphQLRequest.</returns>
        protected abstract GQLRequest CreateRequest();

        /// <summary>
        /// Validates the response object from a subscription. Used to report on
        /// the results of the data received.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns><c>true</c> if the data object is valid, <c>false</c> otherwise.</returns>
        protected abstract bool ValidateResult(TResult response);

        /// <inheritdoc />
        protected override Task ExecuteSingleClientIteration(
            ScriptProfileClientOptions options,
            ClientScriptIterationResults recordTo,
            CancellationToken cancelToken = default)
        {
            if (_iterationsInitiated > 0)
                throw new InvalidOperationException("Subscription clients cannot issue more than one iteration");

            _iterationsInitiated++;

            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().Name);

            var request = this.CreateRequest();

            _client = this.CreateGraphQLClient();
            var stream = _client.CreateSubscriptionStream<TResult>(
                request,
                this.CreateExceptionHandler(recordTo));

            var subscription = new Subscription<TResult>(
                recordTo,
                stream,
                this.ValidateResult);

            subscription.StartListening(this.OnSubscriptionStopped, cancelToken);

            var completionSource = new TaskCompletionSource();

            lock (_subscriptions)
                _subscriptions.Add(subscription, completionSource);

            return completionSource.Task;
        }

        /// <inheritdoc />
        protected override Task ExecuteSingleGraphQLQuery(
            ClientScriptIterationResults recordTo,
            GraphQLHttpClient client,
            CancellationToken cancelToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override ClientScriptIterationResults CreateIterationResultsSet(string resultsId, ScriptProfileClientOptions options)
        {
            return new ClientScriptIterationResults(
                resultsId,
                options.CallsPerIteration,
                ClientScriptResultType.Inbound);
        }

        private void OnSubscriptionStopped(Subscription<TResult> subscription)
        {
            lock (_subscriptions)
            {
                if (_subscriptions.ContainsKey(subscription))
                {
                    var source = _subscriptions[subscription];
                    source.TrySetCanceled();
                    _subscriptions.Remove(subscription);
                    subscription.Dispose();
                }

                _client?.Dispose();
            }
        }

        private Action<Exception> CreateExceptionHandler(ClientScriptIterationResults recordTo)
        {
            return (ex) =>
            {
                if (ex is TaskCanceledException)
                    return;

                recordTo.AddResults("exception", ex.Message);
            };
        }

        /// <inheritdoc />
        protected override Task ExecuteSingleQuery(ClientScriptIterationResults recordTo, CancellationToken cancelToken = default)
        {
            // not used by subscription client
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_isDisposed)
            {
                if (disposing)
                {
                    lock (_subscriptions)
                    {
                        foreach (var kvp in _subscriptions)
                        {
                            kvp.Value.TrySetException(new ObjectDisposedException(kvp.Key.GetType().Name));
                            kvp.Key.Dispose();
                        }

                        _subscriptions.Clear();
                    }

                    _client.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Gets the client instantiation number of this instance.
        /// </summary>
        /// <value>The client number.</value>
        public int ClientNumber { get; }
    }
}