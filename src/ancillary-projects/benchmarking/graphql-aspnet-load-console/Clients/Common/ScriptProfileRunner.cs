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
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A runner that can execute a script profile from start to finish.
    /// </summary>
    public class ScriptProfileRunner
    {
        private readonly ScriptProfileRunnerOptions _runnerOptions;
        private readonly List<ScriptProfileClientOptions> _clientOptions;
        private readonly Dictionary<ScriptProfileClientOptions, List<IClientScript>> _scripts;
        private readonly ReportWriter _writer;
        private readonly object _scriptIdLocker = new object();

        private int _nextScriptNumber;
        private DateTimeOffset _startDate;
        private DateTimeOffset? _endDate;
        private bool _isRunning;
        private bool _isExecuted;
        private int _lastTotalOutboundCallCount;
        private int _lastTotalInboundCallCount;
        private DateTimeOffset _lastCallCountPollTime;

        private double _lastComputedOutboundCallsPerSec;
        private double _lastComputedInboundCallsPerSec;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptProfileRunner" /> class.
        /// </summary>
        /// <param name="runnerOptions">The runner options to configure
        /// the overall run.</param>
        /// <param name="clientOptions">The set of client specific options to configure
        /// the clients that should be executed on this runner.</param>
        public ScriptProfileRunner(
            ScriptProfileRunnerOptions runnerOptions,
            params ScriptProfileClientOptions[] clientOptions)
        {
            _runnerOptions = runnerOptions;

            _clientOptions = new List<ScriptProfileClientOptions>();
            if (clientOptions != null)
                _clientOptions.AddRange(clientOptions);

            _scripts = new Dictionary<ScriptProfileClientOptions, List<IClientScript>>();
            _writer = new ReportWriter();
            _lastCallCountPollTime = DateTimeOffset.UtcNow;
        }

        private int NextScriptNumber()
        {
            lock (_scriptIdLocker)
                return _nextScriptNumber++;
        }

        /// <summary>
        /// Begins executing the profile to its end.
        /// </summary>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        public async Task ExecuteProfile(CancellationToken cancelToken = default)
        {
            if (_isExecuted)
                throw new InvalidOperationException("this runner has already been executed");

            _isExecuted = true;
            _isRunning = true;
            _startDate = DateTimeOffset.UtcNow;

            var executingTasks = new List<Task>();
            var wasCanceled = false;
            try
            {
                // begin the report monitor
                _ = Task.Run(() => UpdateScreenWithReport(cancelToken));

                // startup each client configuration
                foreach (var clientOption in _clientOptions)
                {
                    var task = this.ExecuteClientProfile(clientOption, cancelToken);
                    executingTasks.Add(task);

                    await Task.Delay(_runnerOptions.ClientProfileCooldown, cancelToken);
                }

                // wait for all client profiles to finish
                await Task.WhenAll(executingTasks);
            }
            catch (OperationCanceledException)
            {
                wasCanceled = true;
            }
            finally
            {
                _isRunning = false;
            }

            _endDate = DateTimeOffset.UtcNow;
            PrintReport(true, wasCanceled);
        }

        private async Task ExecuteClientProfile(ScriptProfileClientOptions clientOption, CancellationToken cancelToken = default)
        {
            await Task.Delay(clientOption.StartupDelay);

            var clientScriptTasks = new List<Task>();
            for (var i = 0; i < clientOption.TotalClientInstances; i++)
            {
                var scriptClient = clientOption.ScriptMaker(this.NextScriptNumber());
                lock (_scripts)
                {
                    if (!_scripts.ContainsKey(clientOption))
                        _scripts.Add(clientOption, new List<IClientScript>());

                    _scripts[clientOption].Add(scriptClient);
                }

                var task = scriptClient.ExecuteClientScript(clientOption, cancelToken);
                clientScriptTasks.Add(task);

                await Task.Delay(clientOption.ClientCreationCooldown);
            }

            await Task.WhenAll(clientScriptTasks);
        }

        private async void UpdateScreenWithReport(CancellationToken cancelToken)
        {
            var random = new Random();
            var minDelay = Convert.ToInt32(_runnerOptions.ReportRefreshDelay.Add(TimeSpan.FromMilliseconds(_runnerOptions.ReportRefreshJitterMs * -1)).TotalMilliseconds);
            var maxDelay = Convert.ToInt32(_runnerOptions.ReportRefreshDelay.Add(TimeSpan.FromMilliseconds(_runnerOptions.ReportRefreshJitterMs)).TotalMilliseconds);

            try
            {
                while (!cancelToken.IsCancellationRequested && _isRunning)
                {
                    this.PrintReport(false);

                    var delay = _runnerOptions.ReportRefreshDelay;
                    if (_runnerOptions.ReportRefreshJitterMs != 0)
                        delay = TimeSpan.FromMilliseconds(random.Next(minDelay, maxDelay));

                    await Task.Delay(delay, cancelToken);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void PrintReport(bool printFinalSummary, bool wasCanceled = false)
        {
            _writer.BeginNew();

            this.PrintHeader();

            this.PrintGroupedResults();

            this.PrintThroughput();

            this.PrintErrors();

            if (wasCanceled)
                _writer.WriteLine("---OPERATION TERMINATED---", ConsoleColor.Red);

            if (printFinalSummary)
                this.PrintFinalResults();
            else
                _writer.WriteLine("Press any key to terminate...");

            _writer.ClearRemaining();
        }

        private void PrintHeader()
        {
            _writer.WriteLine($"Profile                : {_runnerOptions.Title}");
            _writer.WriteLine($"Total Client Profiles  : {_clientOptions.Count}");
            _writer.WriteLine($"Total Scripts          : {_clientOptions.Sum(x => x.TotalClientInstances)}");
            _writer.WriteLine();
        }

        private void PrintGroupedResults()
        {
            const int leftColumn = 25;
            const int rightColumn = 35;

            _writer.WritePadRight("Client Group", leftColumn);
            _writer.WritePadLeft("Results", rightColumn);
            _writer.WriteLine();
            _writer.WritePadLeft(string.Empty, leftColumn + rightColumn, '-');
            _writer.WriteLine();

            /* Printout Format for each client profile group
             *
             * {ProfileTitle}              {Script Counts}
             *    {ExeCategory}
             *      {SubCategory1}            {SubCatRes1}
             *      {SubCategory2}            {SubCatRes1}
             *                             {ExeCatPending}
             */
            var scripts = _scripts.ToList();
            foreach (var clientGroup in scripts)
            {
                var clientOptions = clientGroup.Key;
                var clientScripts = clientGroup.Value;

                // {ProfileTitle}
                _writer.WritePadRight(clientGroup.Key.Title, leftColumn);

                // {Script Counts}
                var scriptPrint = string.Empty;
                if (clientScripts.Count < clientOptions.TotalClientInstances)
                    scriptPrint = $"Scpts: {clientScripts.Count} ({clientOptions.TotalClientInstances})";
                else
                    scriptPrint = $"Scpts: {clientScripts.Count}";

                var iterationPrint = string.Empty;
                var existingIterations = clientScripts.Sum(x => x.Results.Count);
                var maxIterations = clientOptions.IterationsPerClient * clientOptions.TotalClientInstances;
                if (existingIterations < maxIterations)
                    iterationPrint = $"Iter: {existingIterations} ({maxIterations})";
                else
                    iterationPrint = $"Iter: {existingIterations}";

                _writer.WritePadLeft($"{{{scriptPrint}, {iterationPrint}}}", rightColumn);
                _writer.WriteLine();

                var executionCategories = clientScripts
                    .GroupBy(x => x.ExecutionCategory)
                    .ToList();
                foreach (var group in executionCategories.OrderBy(x => x.Key))
                {
                    // {ExeCategory}
                    _writer.WritePadRight($"  {group.Key}", leftColumn);
                    _writer.WriteLine();

                    // summarize the iteration results by category
                    var dic = new Dictionary<string, int>();
                    var totalCallsMade = 0;
                    var maxCallsTomake = 0;

                    var results = group.SelectMany(x => x.Results).ToList();
                    foreach (var iterationResult in results)
                    {
                        maxCallsTomake += iterationResult.ExpectedCalls;
                        foreach (var kvp in iterationResult.CountByCategory)
                        {
                            if (!dic.ContainsKey(kvp.Key))
                                dic.Add(kvp.Key, 0);

                            dic[kvp.Key] += kvp.Value;

                            totalCallsMade += kvp.Value;
                        }
                    }

                    if (dic.Count == 0)
                        dic.Add("~No Results~", 0);

                    foreach (var kvp in dic)
                    {
                        // {SubCategory}
                        _writer.WritePadRight($"    {kvp.Key}", leftColumn);

                        // {SubCategoryResult}
                        _writer.WritePadLeft(kvp.Value.ToString(), rightColumn);
                        _writer.WriteLine();
                    }

                    // {ExeCatPending}
                    if (maxCallsTomake - totalCallsMade > 0)
                    {
                        _writer.WritePadRight(string.Empty, leftColumn);
                        _writer.WritePadLeft($"({maxCallsTomake - totalCallsMade} remaining)", rightColumn);
                        _writer.WriteLine();
                    }

                    _writer.WriteLine();
                }
            }
        }

        private void PrintErrors()
        {
            var allErrors = _scripts.SelectMany(x => x.Value)
                .SelectMany(x => x.Results)
                .Where(x => x.Errors.Count > 0)
                .SelectMany(x => x.Errors)
                .Reverse()
                .Take(3)
                .ToList();

            if (allErrors.Count > 0)
            {
                foreach (var error in allErrors)
                    _writer.WriteLine(error, ConsoleColor.Red);
            }
        }

        private void PrintThroughput()
        {
            var elapsedTime = DateTimeOffset.UtcNow - _startDate;

            var currentOutboundCalls = _scripts
                .SelectMany(x => x.Value)
                .SelectMany(x => x.Results)
                .Where(x => x.ResultType == ClientScriptResultType.Outbound)
                .Sum(x => x.CompletedCalls);

            var currentInboundCalls = _scripts
                .SelectMany(x => x.Value)
                .SelectMany(x => x.Results)
                .Where(x => x.ResultType == ClientScriptResultType.Inbound)
                .Sum(x => x.CompletedCalls);

            // only update the calls per second every 15 seconds
            var elapsedTimeThisInterval = DateTimeOffset.UtcNow - _lastCallCountPollTime;
            if (elapsedTimeThisInterval.TotalSeconds > 15)
            {
                var outboundCallsThisInterval = currentOutboundCalls - _lastTotalOutboundCallCount;
                var inboundCallsThisInterval = currentInboundCalls - _lastTotalInboundCallCount;

                var outboundCallsPerSec = outboundCallsThisInterval / elapsedTimeThisInterval.TotalSeconds;
                var inboundCallsPerSec = inboundCallsThisInterval / elapsedTimeThisInterval.TotalSeconds;

                _lastTotalInboundCallCount = currentInboundCalls;
                _lastTotalOutboundCallCount = currentOutboundCalls;
                _lastComputedOutboundCallsPerSec = outboundCallsPerSec;
                _lastComputedInboundCallsPerSec = inboundCallsPerSec;
                _lastCallCountPollTime = DateTimeOffset.UtcNow;
            }

            _writer.WriteLine($"Elapsed Time   : {elapsedTime:mm\\:ss\\.fff}");
            _writer.WriteLine($"Outbound Calls : {currentOutboundCalls} ({_lastComputedOutboundCallsPerSec:0.00}/sec)");
            _writer.WriteLine($"Inbound Calls  : {currentInboundCalls} ({_lastComputedInboundCallsPerSec:0.00}/sec)");
            _writer.WriteLine();
        }

        private void PrintFinalResults()
        {
            _writer.WriteLine("Operation Finished");
            _writer.WriteLine("Press any key to close...");
        }
    }
}