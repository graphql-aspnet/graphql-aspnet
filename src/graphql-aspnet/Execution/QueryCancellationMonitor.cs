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
    using GraphQL.AspNet.Internal.Introspection.Types;

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

        private readonly object _locker;
        private readonly CancellationToken _contextCancelToken;
        private readonly TimeSpan _timeoutPeriod;
        private readonly GraphQueryExecutionContext _context;

        private TaskCompletionSource<int> _publicTimeoutTaskSource;

        private CancellationTokenSource _timeoutTaskCancelSource;
        private CancellationTokenSource _combinedCancelSource;

        private Task _delayTask;
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
            _contextCancelToken = _context.CancellationToken;
        }

        /// <summary>
        /// Issues a new timeout task appropriate for the source context and schema.
        /// </summary>
        public void Start()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(QueryCancellationMonitor));

            _publicTimeoutTaskSource = new TaskCompletionSource<int>();

            // when a timeout is specified
            // combine a timeout specific token
            // with the original, external token
            if (_timeoutPeriod != Timeout.InfiniteTimeSpan)
            {
                _timeoutTaskCancelSource = new CancellationTokenSource();
                _combinedCancelSource = CancellationTokenSource.CreateLinkedTokenSource(
                    _contextCancelToken,
                    _timeoutTaskCancelSource.Token);

                // start the delay timer ticking, but stop it
                // if the context token fires
                _delayTask = Task.Delay(_timeoutPeriod, _timeoutTaskCancelSource.Token)
                .ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled && !_isDisposed)
                    {
                        // if the internal timeout task finishes
                        // cause the public facing task to complete
                        this.SetState(MonitorState.Timeout);
                        _publicTimeoutTaskSource.TrySetResult(0);
                    }
                });
            }

            this.TimeoutTask = _publicTimeoutTaskSource.Task;

            // publish the appropriate cancel token
            this.CancellationToken = _timeoutPeriod == Timeout.InfiniteTimeSpan
                ? _contextCancelToken
                : _combinedCancelSource.Token;
        }

        /// <summary>
        /// Inspects the monitored context and its constituent tokens to determine if anything
        /// is in an aborted state. If an aborted state is found an appropriate cancellation
        /// state is set.
        /// </summary>
        public void AbortIfCancelled()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(QueryCancellationMonitor));

            // stop the timer if the context token requested cancellation
            // along the way and transition the monitor to a canceled state
            if (_contextCancelToken.IsCancellationRequested)
            {
                this.SetState(MonitorState.CancelRequested);

                // kill the delay task if one is running
                _timeoutTaskCancelSource?.Cancel();

                // complete the public timeout task
                _publicTimeoutTaskSource.TrySetResult(0);
            }
        }

        /// <summary>
        /// Attempts to complete this monitor and stop any outstanding timers.
        /// </summary>
        public void Complete()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(QueryCancellationMonitor));

            this.AbortIfCancelled();
            this.SetState(MonitorState.Completed);
        }

        private bool SetState(MonitorState newState)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(QueryCancellationMonitor));

            lock (_locker)
            {
                if (_state == MonitorState.Incomplete)
                {
                    _state = newState;

                    // when a final state is set to the monitor, we can stop
                    // waiting for the timeout to complete
                    _timeoutTaskCancelSource?.Cancel();

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
                    if (_timeoutTaskCancelSource != null)
                    {
                        _timeoutTaskCancelSource.Cancel();
                        _timeoutTaskCancelSource.Dispose();
                    }

                    _combinedCancelSource?.Dispose();

                    if (_publicTimeoutTaskSource?.Task != null)
                    {
                        if (!_publicTimeoutTaskSource.Task.IsCompleted)
                            _publicTimeoutTaskSource.TrySetCanceled();

                        _publicTimeoutTaskSource.Task.Dispose();
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