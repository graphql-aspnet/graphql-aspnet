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
        private readonly CancellationToken _externalCancelToken;
        private readonly TimeSpan _timeoutPeriod;

        private TaskCompletionSource<int> _publicTimeoutTaskSource;

        private CancellationTokenSource _timeoutTaskCancelSource;
        private CancellationTokenSource _combinedCancelSource;

        private Task _delayTask;
        private MonitorState _state;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryCancellationMonitor" /> class.
        /// </summary>
        /// <param name="externalToken">An external token that,if cancelled will automatically end the monitor.</param>
        /// <param name="timeoutPeriod">A period of time after which this monitor will be marked
        /// as timedout and operations cancelled.</param>
        public QueryCancellationMonitor(CancellationToken externalToken = default, TimeSpan? timeoutPeriod = null)
        {
            _timeoutPeriod = timeoutPeriod ?? Timeout.InfiniteTimeSpan;
            _locker = new object();
            _state = MonitorState.Incomplete;
            _externalCancelToken = externalToken;
        }

        /// <summary>
        /// Starts the monitor, if a timeout period was specified or an original external token provided,
        /// the monitor will automaitcally timeout or cancel if either event occurs prior to a call to <see cref="Complete"/>.
        /// </summary>
        public void Start()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(QueryCancellationMonitor));

            if (_state != MonitorState.Incomplete)
                return;

            _publicTimeoutTaskSource = new TaskCompletionSource<int>();

            // when a timeout is specified
            // combine a timeout specific token
            // with the original, external token
            if (_timeoutPeriod != Timeout.InfiniteTimeSpan)
            {
                _timeoutTaskCancelSource = new CancellationTokenSource();
                _combinedCancelSource = CancellationTokenSource.CreateLinkedTokenSource(
                    _externalCancelToken,
                    _timeoutTaskCancelSource.Token);

                // start the delay timer ticking, but stop it
                // if the context token fires
                _delayTask = Task.Delay(_timeoutPeriod, _timeoutTaskCancelSource.Token)
                .ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled && !_isDisposed)
                    {
                        // if the internal timeout task finishes normally
                        // the monitor has timed out
                        this.SetState(MonitorState.Timeout);
                    }
                });
            }

            this.MonitorTask = _publicTimeoutTaskSource.Task;

            if (_externalCancelToken != default)
                _externalCancelToken.Register(this.ExternalToken_Cancelled);

            // publish the appropriate cancel token
            this.CancellationToken = _timeoutPeriod == Timeout.InfiniteTimeSpan
                ? _externalCancelToken
                : _combinedCancelSource.Token;
        }

        private void ExternalToken_Cancelled()
        {
            if (_isDisposed)
                return;

            // stop the timer if the context token requested cancellation
            // along the way and transition the monitor to a canceled state
            if (_externalCancelToken.IsCancellationRequested)
                this.SetState(MonitorState.CancelRequested);
        }

        /// <summary>
        /// Attempts to complete this monitor and stop any outstanding timers.
        /// </summary>
        public void Complete()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(QueryCancellationMonitor));

            this.SetState(MonitorState.Completed);
        }

        private bool SetState(MonitorState newState)
        {
            if (_isDisposed)
                return false;

            lock (_locker)
            {
                if (_state == MonitorState.Incomplete)
                {
                    _state = newState;

                    // when a final state is set to the monitor, we can stop
                    // waiting for the timeout to complete
                    if (_timeoutTaskCancelSource != null && !_timeoutTaskCancelSource.IsCancellationRequested)
                        _timeoutTaskCancelSource.Cancel();

                    // complete the monitor task
                    _publicTimeoutTaskSource.TrySetResult(0);

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
        /// Gets the cancellation token governing this monitor. This token will request cancellation if
        /// the timeout period expires or if the original, external token requests cancellation.
        /// </summary>
        /// <value>The cancellation token.</value>
        public CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// Gets a task representing the timeout operation this monitoring is watching. This task will be completed
        /// when any of the following occur:<br />
        /// - The timeout period expires<br/>
        /// - The external token supplied in the constructor requests a cancellation<br/>
        /// - The .Complete() method is called.<br/>
        /// .
        /// </summary>
        /// <value>The timeout task.</value>
        public Task MonitorTask { get; private set; }

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