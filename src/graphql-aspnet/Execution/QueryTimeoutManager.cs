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
    /// A helper class to track all the various bits governing the execution of query context, a runtime
    /// timeout, and a governing, preexisting cancellation token.
    /// </summary>
    internal sealed class QueryTimeoutManager : IDisposable
    {
        private enum MonitorState
        {
            Incomplete = 0,
            Timeout = 1,
            CancelRequested = 2,
            Completed = 3,
        }

        private readonly TimeSpan? _timeoutPeriod;
        private readonly GraphQueryExecutionContext _context;

        private bool _disposedValue;
        private CancellationTokenSource _runtimeCancelSource;
        private CancellationTokenSource _combinedCancelSource;
        private MonitorState _state;
        private object _locker;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTimeoutManager" /> class.
        /// </summary>
        /// <param name="context">The context to monitor.</param>
        /// <param name="timeoutPeriod">A period of time after which this monitor will be marked
        /// as timedout and operations cancelled.</param>
        public QueryTimeoutManager(GraphQueryExecutionContext context, TimeSpan? timeoutPeriod)
        {
            _context = Validation.ThrowIfNullOrReturn(context, nameof(context));

            _timeoutPeriod = timeoutPeriod;
            _locker = new object();
            _state = MonitorState.Incomplete;
        }

        /// <summary>
        /// Issues a new timeout task appropriate for the source context and schema.
        /// </summary>
        public void Start()
        {
            // register a handler to determine if an external actor
            // caused the process to finish
            _context.CancellationToken.Register(() =>
            {
                this.SetState(MonitorState.CancelRequested);
            });

            // When a timeout period is not specified
            // skip all internal timeout governing operations
            if (!_timeoutPeriod.HasValue)
            {
                this.CancellationToken = _context.CancellationToken;
                this.TimeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, this.CancellationToken);
                return;
            }

            _runtimeCancelSource = new CancellationTokenSource();

            // combine the runtime timeout with the original, external token provided to the runtime
            _combinedCancelSource = CancellationTokenSource.CreateLinkedTokenSource(
                _context.CancellationToken,
                _runtimeCancelSource.Token);

            // start the timeout ticking
            this.TimeoutTask = Task.Delay(_timeoutPeriod.Value, _context.CancellationToken)
                .ContinueWith(x =>
                {
                    this.SetState(MonitorState.Timeout);
                    _runtimeCancelSource.Cancel();
                });

            // publish the cancel token
            this.CancellationToken = _combinedCancelSource.Token;
        }

        /// <summary>
        /// Attempts to complete this monitor and stop any outstanding timers.
        /// </summary>
        public void Complete()
        {
            this.SetState(MonitorState.Completed);
        }

        private bool SetState(MonitorState newState)
        {
            lock (_locker)
            {
                if (_state == MonitorState.Incomplete)
                {
                    _state = newState;
                    return true;
                }

                return false;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_combinedCancelSource != null)
                        _combinedCancelSource.Dispose();
                    if (_runtimeCancelSource != null)
                        _runtimeCancelSource.Dispose();
                }

                _disposedValue = true;
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
        public bool CancellationRequested => _state == MonitorState.CancelRequested;

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