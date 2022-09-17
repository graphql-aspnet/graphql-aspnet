// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;

    /// <summary>
    /// A helper class to manage the various external pressures on the execution of query context.
    /// (e.g. a runtime query timeout and/or a governing cancellation token).
    /// </summary>
    internal sealed class QueryCancellationMonitor : IDisposable
    {
        private enum MonitorState
        {
            Incomplete = 0,
            Timeout = 1,
            CancelRequested = 2,
            Completed = 3,
        }

        private readonly CancellationToken _governingToken;
        private readonly TimeSpan _timeoutPeriod;
        private readonly GraphQueryExecutionContext _context;
        private readonly object _locker;
        private readonly CancellationTokenSource _timeoutTaskSource;

        private CancellationTokenSource _runtimeCancelSource;
        private CancellationTokenSource _combinedCancelSource;
        private MonitorState _state;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryCancellationMonitor" /> class.
        /// </summary>
        /// <param name="context">The context to monitor.</param>
        /// <param name="timeoutPeriod">A period of time after which this monitor will be marked
        /// as timedout and operations cancelled.</param>
        public QueryCancellationMonitor(GraphQueryExecutionContext context, TimeSpan? timeoutPeriod)
        {
            _context = Validation.ThrowIfNullOrReturn(context, nameof(context));

            _timeoutPeriod = timeoutPeriod ?? Timeout.InfiniteTimeSpan;
            _locker = new object();
            _state = MonitorState.Incomplete;
            _governingToken = _context.CancellationToken;

            _timeoutTaskSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Issues a new timeout task appropriate for the source context and schema.
        /// </summary>
        public void Start()
        {
            // when a timeout is specified by the runtime
            // combine the runtime defined timeout token
            // with the original, external token
            if (_timeoutPeriod != Timeout.InfiniteTimeSpan)
            {
                _runtimeCancelSource = new CancellationTokenSource();
                _combinedCancelSource = CancellationTokenSource.CreateLinkedTokenSource(
                    _governingToken,
                    _runtimeCancelSource.Token);
            }

            // start the timeout ticking
            this.TimeoutTask = Task.Delay(_timeoutPeriod, _timeoutTaskSource.Token)
                .ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled && !_isDisposed)
                    {
                        // if the timeout task finishes
                        // cause the runtime token to request cancellation
                        // this will trigger the combined token if one was made
                        // and any listeners to the token will recieve the cancel notice
                        _runtimeCancelSource?.Cancel();
                    }
                });

            // publish the appropriate cancel token
            this.CancellationToken = _timeoutPeriod == Timeout.InfiniteTimeSpan
                ? _governingToken
                : _combinedCancelSource.Token;
        }

        /// <summary>
        /// Inspects the monitored context and its constituent tokens to determine if anything
        /// is in an aborted state. If an aborted state is found an appropriate cancellation
        /// state is set.
        /// </summary>
        public void AbortIfCancelled()
        {
            if (!_isDisposed)
            {
                if (_runtimeCancelSource != null && _runtimeCancelSource.IsCancellationRequested)
                {
                    this.SetState(MonitorState.Timeout);
                }
                else if (_governingToken.IsCancellationRequested)
                {
                    this.SetState(MonitorState.CancelRequested);
                }
            }
        }

        /// <summary>
        /// Attempts to complete this monitor and stop any outstanding timers.
        /// </summary>
        public void Complete()
        {
            if (!_isDisposed)
            {
                this.AbortIfCancelled();
                this.SetState(MonitorState.Completed);
            }
        }

        private bool SetState(MonitorState newState)
        {
            lock (_locker)
            {
                if (_state == MonitorState.Incomplete)
                {
                    _state = newState;

                    // when a final state is set to the monitor, we can stop
                    // waiting for the timeout to complete
                    if (!_isDisposed)
                        _timeoutTaskSource.Cancel();
                    return true;
                }

                return false;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_combinedCancelSource != null)
                    {
                        _combinedCancelSource.Cancel();
                        _combinedCancelSource.Dispose();
                    }

                    if (_runtimeCancelSource != null)
                    {
                        _runtimeCancelSource.Cancel();
                        _runtimeCancelSource.Dispose();
                    }

                    if (_timeoutTaskSource != null)
                    {
                        _timeoutTaskSource.Cancel();
                        _timeoutTaskSource.Dispose();
                    }
                }

                _isDisposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the cancellation token governing this monitored context.
        /// </summary>
        /// <value>The cancellation token.</value>
        public CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// Gets a task representing the timeout operation this monitoring is watching. Useful when using
        /// <c>Task.WhenAny</c> along with another task.
        /// </summary>
        /// <value>The timeout task.</value>
        public Task TimeoutTask { get; private set; }

        /// <summary>
        /// Gets a value indicating whether cancellation was explicitly requested prior to a timeout.
        /// </summary>
        /// <value><c>true</c> if cancellation was requested; otherwise, <c>false</c>.</value>
        public bool IsCancelled => _state == MonitorState.CancelRequested;

        /// <summary>
        /// Gets a value indicating whether a timeout occured prior to the completion of the monitor.
        /// </summary>
        /// <value><c>true</c> if [was timed out]; otherwise, <c>false</c>.</value>
        public bool IsTimedOut => _state == MonitorState.Timeout;

        /// <summary>
        /// Gets a value indicating whether this monitor completed before any timeout occured.
        /// </summary>
        /// <value><c>true</c> if the monitor completed successfully; otherwise, <c>false</c>.</value>
        public bool IsCompleted => _state == MonitorState.Completed;

        /// <summary>
        /// Gets a value indicating whether this monitor is still running and tasks should
        /// continue to execute.
        /// </summary>
        /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
        public bool IsRunning => _state == MonitorState.Incomplete;
    }
}