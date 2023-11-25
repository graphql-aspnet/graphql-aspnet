﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Metrics
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Response;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.Response;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A metrics package that tracks according to the apollo tracing standard.
    /// spec: <see href="https://github.com/apollographql/apollo-tracing/" /> .
    /// </summary>
    [DebuggerDisplay("Phases = {PhaseEntries.Count}, Resolvers = {ResolverEntries.Count}")]
    internal class ApolloTracingMetricsV1 : IQueryExecutionMetrics, IDisposable
    {
        private const int VERSION = 1;

        private ConcurrentDictionary<string, ApolloMetricsEntry> _phaseEntries;
        private ConcurrentDictionary<GraphFieldExecutionContext, ApolloMetricsEntry> _resolverEntries;

        private ISchema _schema;
        private Stopwatch _watch;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloTracingMetricsV1" /> class.
        /// </summary>
        /// <param name="schema">The schema being profiled.</param>
        public ApolloTracingMetricsV1(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _phaseEntries = new ConcurrentDictionary<string, ApolloMetricsEntry>();
            _resolverEntries = new ConcurrentDictionary<GraphFieldExecutionContext, ApolloMetricsEntry>();
        }

        /// <inheritdoc />
        public virtual void Start()
        {
            _watch = Stopwatch.StartNew();
            this.StartDate = DateTimeOffset.UtcNow;
        }

        /// <inheritdoc />
        public virtual void End()
        {
            this.TotalTicks = _watch.ElapsedTicks;
            foreach (var item in _phaseEntries.Values.Concat(_resolverEntries.Values))
            {
                if (item.EndOffsetTicks == 0)
                    item.EndOffsetTicks = this.TotalTicks;
            }
        }

        /// <inheritdoc />
        public virtual void StartPhase(string phase)
        {
            if (string.IsNullOrWhiteSpace(phase))
                return;

            if (!_phaseEntries.ContainsKey(phase))
            {
                var phaseEntry = new ApolloMetricsEntry();
                phaseEntry.StartOffsetTicks = _watch.ElapsedTicks;

                _phaseEntries.TryAdd(phase, phaseEntry);
            }
        }

        /// <inheritdoc />
        public virtual void EndPhase(string phase)
        {
            if (string.IsNullOrWhiteSpace(phase))
                return;

            var endTime = _watch.ElapsedTicks;
            if (_phaseEntries.TryGetValue(phase, out var entry))
            {
                if (entry.EndOffsetTicks == 0)
                    entry.EndOffsetTicks = endTime;
            }
        }

        /// <inheritdoc />
        public virtual void BeginFieldResolution(GraphFieldExecutionContext context)
        {
            if (context == null)
                return;

            var startTime = _watch.ElapsedTicks;
            var entry = new ApolloMetricsEntry()
            {
                StartOffsetTicks = startTime,
            };

            _resolverEntries.TryAdd(context, entry);
        }

        /// <inheritdoc />
        public virtual void EndFieldResolution(GraphFieldExecutionContext context)
        {
            if (context == null)
                return;

            var endTime = _watch.ElapsedTicks;
            if (_resolverEntries.TryGetValue(context, out var entry))
            {
                entry.EndOffsetTicks = endTime;
            }
        }

        /// <inheritdoc />
        public virtual IQueryResponseFieldSet GenerateResult()
        {
            var results = new ResponseFieldSet();
            results.AddSingleValue("version", VERSION);

            // the apollo tracing specification says the date MUST be in RFC3339 format
            // do not leave the formatting up to the serializer.
            results.AddSingleValue("startTime", this.StartDate);
            results.AddSingleValue("endTime", this.StartDate.AddTicks(this.TotalTicks));
            results.AddSingleValue("duration", this.TotalTicks);

            if (_phaseEntries.TryGetValue(ApolloExecutionPhase.PARSING, out var entry))
                results.Add("parsing", this.GeneratePhaseResult(entry));
            if (_phaseEntries.TryGetValue(ApolloExecutionPhase.VALIDATION, out entry))
                results.Add("validation", this.GeneratePhaseResult(entry));

            results.Add("execution", this.GenerateExecutionResult());

            var finalResult = new ResponseFieldSet();
            finalResult.Add("tracing", results);
            return finalResult;
        }

        /// <summary>
        /// Generates the phase metrics result for inclusion in the output json.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>System.Object.</returns>
        private IQueryResponseItem GeneratePhaseResult(ApolloMetricsEntry entry)
        {
            var dictionary = new ResponseFieldSet();
            dictionary.AddSingleValue("startOffset", entry.StartOffsetNanoseconds);
            dictionary.AddSingleValue("duration", entry.DurationNanoSeconds);
            return dictionary;
        }

        /// <summary>
        /// Generates the execution result, a list of individual results for each resolver tracked.
        /// </summary>
        /// <returns>IDictionary&lt;System.String, System.Object&gt;.</returns>
        private IQueryResponseFieldSet GenerateExecutionResult()
        {
            // apollo tracing does not specifiy providing startOffset and duration keys for the execution
            // phase, just output the resolvers array
            var list = new ResponseList();
            foreach (var timeEntry in _resolverEntries.OrderBy(x => x.Value.StartOffsetTicks))
            {
                var resolverEntry = new ResponseFieldSet();
                resolverEntry.Add("path", new ResponseList(timeEntry
                    .Key
                    .Request
                    .Origin
                    .Path
                    .ToArray()
                    .Select(x => new ResponseSingleValue(x))));

                IGraphType parentType = null;

                if (timeEntry.Key?.Request?.Field?.Mode == FieldResolutionMode.Batch)
                {
                    var parentName = timeEntry.Key?.Request?.Field?.ItemPath?.Parent?.Name;
                    if (!string.IsNullOrWhiteSpace(parentName))
                        parentType = _schema.KnownTypes.FindGraphType(parentName);
                }
                else
                {
                    parentType = _schema.KnownTypes.FindGraphType(timeEntry.Key.Request?.Data?.Value);
                }

                if (parentType != null)
                {
                    resolverEntry.AddSingleValue("parentType", parentType.Name);
                }

                resolverEntry.AddSingleValue("fieldName", timeEntry.Key.Request.Field.Name);
                resolverEntry.AddSingleValue("returnType", timeEntry.Key.Request.Field.TypeExpression.ToString());
                resolverEntry.AddSingleValue("startOffset", timeEntry.Value.StartOffsetNanoseconds);
                resolverEntry.AddSingleValue("duration", timeEntry.Value.DurationNanoSeconds);
                list.Add(resolverEntry);
            }

            var dictionary = new ResponseFieldSet();
            dictionary.Add("resolvers", list);
            return dictionary;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _resolverEntries.Clear();
                _phaseEntries.Clear();

                _resolverEntries = null;
                _phaseEntries = null;
                _schema = null;
                _watch = null;
            }
        }

        /// <summary>
        /// Gets the phase entries recorded during the execution run.
        /// </summary>
        /// <value>The phase entries.</value>
        public IReadOnlyDictionary<string, ApolloMetricsEntry> PhaseEntries => _phaseEntries;

        /// <summary>
        /// Gets the resolver entries recorded during the execution run.
        /// </summary>
        /// <value>The resolver entries.</value>
        public IReadOnlyDictionary<GraphFieldExecutionContext, ApolloMetricsEntry> ResolverEntries => _resolverEntries;

        /// <summary>
        /// Gets the datetime when this metrics session was started.
        /// </summary>
        /// <value>The start date.</value>
        public DateTimeOffset StartDate { get; private set; }

        /// <summary>
        /// Gets the total number of ticks that elapsed during the session.
        /// </summary>
        /// <value>The total ticks.</value>
        public long TotalTicks { get; private set; }
    }
}