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
    using System.Threading;
    using GraphQL;
    using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common;

    /// <summary>
    /// A monitor on a single subscription within a client.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to come through the
    /// subscirption.</typeparam>
    public class Subscription<TResult> : IDisposable
    {
        private readonly Func<TResult, bool> _validator;
        private readonly ClientScriptIterationResults _resultAggregate;
        private IDisposable _subscriptionStream;
        private IObservable<GraphQLResponse<TResult>> _observable;
        private bool _isDisposed;
        private Action<Subscription<TResult>> _subStopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription{TResult}" /> class.
        /// </summary>
        /// <param name="resultAggregate">The object to record results to.</param>
        /// <param name="stream">The governing event stream stream.</param>
        /// <param name="validator">The validator function to
        /// process the result of a received event.</param>
        public Subscription(
            ClientScriptIterationResults resultAggregate,
            IObservable<GraphQLResponse<TResult>> stream,
            Func<TResult, bool> validator)
        {
            _observable = stream;
            _validator = validator;
            _resultAggregate = resultAggregate;
        }

        /// <summary>
        /// Opens the connection and starts listening for events.
        /// </summary>
        /// <param name="subscriptionStopped">A method to be called
        /// when this subscription stops listening.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public void StartListening(
            Action<Subscription<TResult>> subscriptionStopped,
            CancellationToken cancelToken = default)
        {
            _subStopped = subscriptionStopped;
            if (_observable != null && _subscriptionStream == null)
            {
                this.IsActive = true;
                _subscriptionStream = _observable.Subscribe(this.CaptureEvent);
                cancelToken.Register(this.StopSubscription);
            }
        }

        private void CaptureEvent(GraphQLResponse<TResult> response)
        {
            if (response.Errors != null && response.Errors.Length > 0)
            {
                _resultAggregate.AddResults("graphql-error", response.Errors[0].Message);
            }
            else
            {
                if (_validator(response.Data))
                    _resultAggregate.AddResults("success");
                else
                    _resultAggregate.AddResults("invalid");
            }

            if (_resultAggregate.CompletedCalls >= _resultAggregate.ExpectedCalls)
            {
                this.StopSubscription();
            }
        }

        /// <summary>
        /// Stops the subscription from receiving events permanently.
        /// </summary>
        public void StopSubscription()
        {
            this.IsActive = false;
            _subscriptionStream?.Dispose();
            _subscriptionStream = null;
            _observable = null;
            if (_subStopped != null)
            {
                _subStopped(this);
                _subStopped = null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active
        /// and receiving events.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    this.StopSubscription();
                }

                _isDisposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}