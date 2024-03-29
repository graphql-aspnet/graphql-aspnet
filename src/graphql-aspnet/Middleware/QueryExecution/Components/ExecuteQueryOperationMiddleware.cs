﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.QueryExecution.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryFragmentSteps;

    /// <summary>
    /// Begins executing the top level fields, of the operation on the context, through the field execution pipeline.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware exists for.</typeparam>
    public class ExecuteQueryOperationMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private class FieldPipelineInvocation
        {
            public Task Task { get; set; }

            public FieldDataItem DataItem { get; set; }

            public GraphFieldExecutionContext FieldContext { get; set; }
        }

        private readonly ISchemaPipeline<TSchema, GraphFieldExecutionContext> _fieldExecutionPipeline;
        private readonly TSchema _schema;
        private readonly bool _debugMode;
        private readonly TimeSpan? _queryTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteQueryOperationMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="fieldExecutionPipeline">The field execution pipeline.</param>
        public ExecuteQueryOperationMiddleware(TSchema schema, ISchemaPipeline<TSchema, GraphFieldExecutionContext> fieldExecutionPipeline)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _debugMode = _schema.Configuration.ExecutionOptions.DebugMode;
            _queryTimeout = _schema.Configuration.ExecutionOptions.QueryTimeout;
            _fieldExecutionPipeline = Validation.ThrowIfNullOrReturn(fieldExecutionPipeline, nameof(fieldExecutionPipeline));
        }

        /// <inheritdoc />
        public async Task InvokeAsync(QueryExecutionContext context, GraphMiddlewareInvocationDelegate<QueryExecutionContext> next, CancellationToken cancelToken)
        {
            context.Metrics?.StartPhase(ApolloExecutionPhase.EXECUTION);
            if (context.IsValid && !context.IsCancelled && context.QueryPlan != null)
            {
                await this.ExecuteOperationAsync(context).ConfigureAwait(false);
            }

            await next(context, cancelToken).ConfigureAwait(false);
            context.Metrics?.EndPhase(ApolloExecutionPhase.EXECUTION);
        }

        private async Task ExecuteOperationAsync(QueryExecutionContext context)
        {
            // create a manager that will monitor both the governing token passed
            // on the context as well as the configured timeout for the schema
            using var monitor = new QueryCancellationMonitor(context.CancellationToken, _queryTimeout);

            var expectedTopLevelFieldCount = context
                .QueryPlan
                .Operation
                .FieldContexts
                .Count;

            var operation = context.QueryPlan.Operation;
            var fieldInvocations = new List<FieldPipelineInvocation>(expectedTopLevelFieldCount);
            var fieldInvocationTasks = new List<Task>(expectedTopLevelFieldCount);

            monitor.Start();
            context.CancellationToken = monitor.CancellationToken;

            // Step 0
            // --------------------------

            // top level mutation operatons must be executed in sequential order
            // due to potential side effects on the underlying data
            // https://graphql.github.io/graphql-spec/October2021/#sec-Mutation
            //
            // or if we're in debug mode isolate all type level contexts
            bool contextsMustBeIsolated = _debugMode || operation.OperationType == GraphOperationType.Mutation;
            var contextList = operation.FieldContexts.ToList();

            // Step 1
            // --------------------------
            // Begin a field execution pipeline for each top level field
            // those fields will then call their child fields in turn
            foreach (var item in contextList)
            {
                // if at any point the context token signals a cancellation
                // the monitor will switch out of a running state,
                // at which point we can just stop executing
                if (!monitor.IsRunning)
                    break;

                var path = new SourcePath();
                path.AddFieldName(item.Name);

                object dataSourceValue;

                // fetch the source data value to use for the field invocation
                // attempt to retrieve from the master context if it was supplied by the pipeline
                // invoker, otherwise generate a root source
                if (!context.DefaultFieldSources.TryRetrieveSource(item.Field, out dataSourceValue))
                    dataSourceValue = this.GenerateRootSourceData(operation.OperationType);

                var topLevelDataItem = new FieldDataItem(item, dataSourceValue, path);

                var sourceData = new FieldDataItemContainer(dataSourceValue, path, topLevelDataItem);

                var fieldRequest = new GraphFieldRequest(
                    context.QueryRequest,
                    item,
                    sourceData,
                    new SourceOrigin(item.Origin.Location, path));

                var fieldContext = new GraphFieldExecutionContext(
                    context,
                    fieldRequest,
                    context.ResolvedVariables,
                    context.DefaultFieldSources,
                    resultCapacity: 1);

                var fieldTask = _fieldExecutionPipeline.InvokeAsync(
                    fieldContext,
                    monitor.CancellationToken);

                var pipelineInvocation = new FieldPipelineInvocation()
                {
                    Task = fieldTask,
                    DataItem = topLevelDataItem,
                    FieldContext = fieldContext,
                };

                fieldInvocations.Add(pipelineInvocation);
                fieldInvocationTasks.Add(pipelineInvocation.Task);

                if (contextsMustBeIsolated)
                {
                    // await the isolated task to prevent any potential paralellization
                    // by the task system but not in such a way that a faulted task would
                    // throw an exception. Allow the reslts (exceptions included) to be
                    // captured on the task and handled by the rest of the middleware operation
                    //
                    // while awaiting an isolated task the monitor may complete
                    // exit and stop if so
                    await Task.WhenAny(fieldTask, monitor.MonitorTask).ConfigureAwait(false);
                }
            }

            // Step 2
            // -----------------------------------------
            // await all outstanding tasks and hope they finish before the timer
            if (monitor.IsRunning)
            {
                var fieldPipelineTasksWrapper = Task.WhenAll(fieldInvocationTasks);
                await Task.WhenAny(fieldPipelineTasksWrapper, monitor.MonitorTask).ConfigureAwait(false);

                // finalize the process, indicating all outstanding tasks
                // are completed, cancelled or timed out
                //
                // if the monitor task finished first, indicating a timeout, it will
                // auto flag the monitor as timed out.
                monitor.Complete();
            }

            // Step 3
            // -----------------------------------------
            // gather the results of all tasks executed
            if (monitor.IsCompleted)
            {
                var toThrow = new List<FieldPipelineInvocation>();

                // All field resolutions completed within the timeout period.
                // capture the reslts
                foreach (var invocation in fieldInvocations)
                {
                    if (invocation.Task.IsFaulted)
                        toThrow.Add(invocation);

                    // load the reslts of each field (in order) to the context
                    // for further processing
                    context.FieldResults.Add(invocation.DataItem);
                    context.Messages.AddRange(invocation.FieldContext.Messages);
                }

                // Consider that the task(s) may have faulted or been canceled causing them to complete
                // incorrectly. "re-await" a single failure or aggregate many failues so that
                // any exceptions/cancellation are rethrown correctly.
                // https://stackoverflow.com/questions/4238345/asynchronously-wait-for-taskt-to-complete-with-timeout
                if (toThrow.Count == 1)
                {
                    await toThrow[0].Task.ConfigureAwait(false);
                }
                else if (toThrow.Count > 1)
                {
                    var aggException = new AggregateException(toThrow.SelectMany(x => x.Task.Exception.InnerExceptions).ToArray());
                    throw aggException;
                }
            }

            // log an appropriate error message if needed
            if (monitor.IsCancelled)
            {
                context.Messages.Critical(
                    "The execution was canceled prior to completion of the requested query.",
                    Constants.ErrorCodes.OPERATION_CANCELED);

                context.Logger?.RequestCancelled(context);
            }

            if (monitor.IsTimedOut)
            {
                context.Messages.Critical(
                    $"The execution timed out prior to completion of the requested query. (Allowed Time: {_queryTimeout.Value.TotalSeconds} seconds)",
                    Constants.ErrorCodes.OPERATION_TIMEOUT);

                context.Logger?.RequestTimedOut(context);
            }
        }

        /// <summary>
        /// Generates a "top level" source data item representing the operation root
        /// under which a field context should be executed.
        /// </summary>
        /// <param name="operationType">Type of the operation.</param>
        /// <returns>System.Object.</returns>
        private object GenerateRootSourceData(GraphOperationType operationType)
        {
            if (_schema.Operations.TryGetValue(operationType, out var rootOperation))
            {
                return new VirtualResolvedObject(rootOperation.Name);
            }
            else
            {
                return new object();
            }
        }
    }
}