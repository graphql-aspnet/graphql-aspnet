// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;

    /// <summary>
    /// An ssync friendly Timer implementation. Provides a mechanism for executing an
    /// async method on a thread pool thread at specified intervals.
    /// </summary>
    /// <remarks>
    /// Adapted from: (https://codereview.stackexchange.com/questions/196635/async-friendly-timer).
    /// </remarks>
    public sealed class TimerAsync : IDisposable
    {
        /// <summary>
        /// Occurs when an error is raised in the scheduled action
        /// </summary>
        public event EventHandler<Exception> Error;

        private readonly Func<CancellationToken, Task> _scheduledAction;
        private readonly TimeSpan _initialWaitTime;
        private readonly TimeSpan _interval;
        private readonly SemaphoreSlim _semaphore;
        private readonly bool _canStartNextActionBeforePreviousIsCompleted;

        private CancellationTokenSource _cancellationSource;
        private Task _scheduledTask;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerAsync"/> class.
        /// </summary>
        /// <param name="scheduledAction">A delegate representing a method to be executed.</param>
        /// <param name="initialWaitTime">The amount of time to delay befoe <paramref name="scheduledAction"/> is invoked for the first time.</param>
        /// <param name="interval">The time interval between invocations of the <paramref name="scheduledAction"/>.</param>
        /// <param name="canStartNextActionBeforePreviousIsCompleted">Whether or not the interval starts at the end of the previous scheduled action or at precise points in time.</param>
        public TimerAsync(Func<CancellationToken, Task> scheduledAction, TimeSpan initialWaitTime, TimeSpan interval, bool canStartNextActionBeforePreviousIsCompleted = false)
        {
            _scheduledAction = scheduledAction ?? throw new ArgumentNullException(nameof(scheduledAction));

            if (initialWaitTime < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(initialWaitTime), "Initial wait time time must be greater than or equal to zero.");
            _initialWaitTime = initialWaitTime;

            if (interval < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than or equal to zero.");

            _interval = interval;
            _canStartNextActionBeforePreviousIsCompleted = canStartNextActionBeforePreviousIsCompleted;
            _semaphore = new SemaphoreSlim(1);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TimerAsync"/> class.
        /// </summary>
        ~TimerAsync()
        {
            Dispose(false);
        }

        /// <summary>
        /// Starts the timer. This method is non-blocking.
        /// </summary>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(this.GetType().FriendlyName());

            _semaphore.Wait();

            try
            {
                if (this.IsRunning)
                    return;

                _cancellationSource = new CancellationTokenSource();
                _scheduledTask = RunScheduledActionAsync();
                this.IsRunning = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Begins a task to end the timer. This task will return when the timer
        /// has completed any outstanding iterations and has successfully stopped.
        /// </summary>
        /// <returns>A task that completes when the timer is stopped.</returns>
        public async Task StopAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!this.IsRunning)
                    return;

                _cancellationSource.Cancel();

                await _scheduledTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                this.IsRunning = false;
                _semaphore.Release();
            }
        }

        private Task RunScheduledActionAsync()
        {
            return Task.Run(
                async () =>
                {
                    try
                    {
                        await Task.Delay(_initialWaitTime, _cancellationSource.Token).ConfigureAwait(false);

                        while (true)
                        {
                            if (_canStartNextActionBeforePreviousIsCompleted)
#pragma warning disable 4014
                                _scheduledAction(_cancellationSource.Token);
#pragma warning restore 4014
                            else
                                await _scheduledAction(_cancellationSource.Token).ConfigureAwait(false);

                            await Task.Delay(_interval, _cancellationSource.Token).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            this.Error?.Invoke(this, ex);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    finally
                    {
                        this.IsRunning = false;
                    }
                }, _cancellationSource.Token);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            // NOTE: release unmanaged resources here
            if (disposing)
            {
                _cancellationSource?.Dispose();
                _semaphore?.Dispose();
            }

            _disposed = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets a value indicating whether the timer is currently running.
        /// </summary>
        /// <value><c>true</c> if the timer is running; otherwise, <c>false</c>.</value>
        public bool IsRunning { get; private set; }
    }
}
