// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common
{

    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A base script to execute a query of some type through a controlled
    /// set of iterations and calls.
    /// </summary>
    public abstract class ClientScriptBase : IClientScript, IDisposable
    {
        private List<ClientScriptIterationResults> _results;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientScriptBase" /> class.
        /// </summary>
        /// <param name="scriptNumber">The overall script number assigned to this instance.</param>
        /// <param name="executionCategory">The execution catgory this script falls under.</param>
        /// <param name="categorySuffix">The category suffix to apply to this individual script.</param>
        protected ClientScriptBase(
            int scriptNumber,
            string executionCategory,
            string categorySuffix = "")
        {
            this.ClientId = scriptNumber.ToString("000#");
            _results = new List<ClientScriptIterationResults>();

            if (!string.IsNullOrEmpty(categorySuffix))
                this.ExecutionCategory = $"{executionCategory}-{categorySuffix}";
            else
                this.ExecutionCategory = executionCategory;
        }

        /// <summary>
        /// Creates a new results object to store the results of a single iteration.
        /// </summary>
        /// <param name="resultsId">The id pre-assigned to the iteration.</param>
        /// <param name="options">The set of configuration options governing
        /// the execution of hte cient profile.</param>
        /// <returns>ClientScriptResults.</returns>
        protected virtual ClientScriptIterationResults CreateIterationResultsSet(string resultsId, ScriptProfileClientOptions options)
        {
            return new ClientScriptIterationResults(
                resultsId,
                options.CallsPerIteration,
                ClientScriptResultType.Outbound);
        }

        /// <inheritdoc />
        public async Task ExecuteClientScript(
            ScriptProfileClientOptions options,
            CancellationToken cancelToken = default)
        {
            var iterationsCompleted = 0;
            var iterationTasks = new List<Task>();
            try
            {
                while (!cancelToken.IsCancellationRequested
                    && iterationsCompleted < options.IterationsPerClient)
                {
                    var newIterationResults = this.CreateIterationResultsSet(
                        $"{this.ClientId}-{iterationsCompleted}",
                        options);

                    _results.Add(newIterationResults);
                    var task = ExecuteSingleClientIteration(options, newIterationResults, cancelToken);
                    iterationTasks.Add(task);
                    await Task.Delay(options.IterationCooldown, cancelToken);
                    iterationsCompleted++;
                }

                await Task.WhenAll(iterationTasks);
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        /// Executes a full iteration of the work this script should execute.
        /// </summary>
        /// <param name="options">The configuration options used to setup the iteration.</param>
        /// <param name="recordTo">The results set that metrics should be recorded to.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task ExecuteSingleClientIteration(ScriptProfileClientOptions options, ClientScriptIterationResults recordTo, CancellationToken cancelToken = default)
        {
            var minDelay = options.MinDelayBetweenCallsMs;
            var maxDelay = options.MaxDelayBetweenCallsMs;
            var rando = new Random(DateTime.UtcNow.Millisecond);

            var singleCallTasks = new List<Task>();
            while (!cancelToken.IsCancellationRequested
                && singleCallTasks.Count < options.CallsPerIteration)
            {
                var task = this.ExecuteSingleQuery(recordTo, cancelToken);

                singleCallTasks.Add(task);

                if (maxDelay > 0)
                    await Task.Delay(rando.Next(minDelay, maxDelay), cancelToken);
            }

            await Task.WhenAll(singleCallTasks);
        }

        /// <summary>
        /// Records the exception to the results set.
        /// </summary>
        /// <param name="recordTo">The results set to record to.</param>
        /// <param name="ex">The ex.</param>
        protected void RecordException(ClientScriptIterationResults recordTo, Exception ex)
        {
            switch (ex)
            {
                case TaskCanceledException _:
                    break;

                case HttpRequestException _:
                    recordTo.AddResults("httpException");
                    break;

                default:
                    recordTo.AddResults("exception", ex.Message);
                    break;
            }
        }

        /// <summary>
        /// Executes the one query.
        /// </summary>
        /// <param name="recordTo">The record to.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>System.Threading.Tasks.Task.</returns>
        protected abstract Task ExecuteSingleQuery(ClientScriptIterationResults recordTo, CancellationToken cancelToken = default);

        /// <inheritdoc />
        public string ClientId { get; }

        /// <inheritdoc />
        public IReadOnlyList<ClientScriptIterationResults> Results => _results;

        /// <inheritdoc />
        public string ExecutionCategory { get; }

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
    }
}