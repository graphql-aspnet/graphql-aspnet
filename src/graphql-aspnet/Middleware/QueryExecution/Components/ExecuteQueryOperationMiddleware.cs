// *************************************************************
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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Variables;

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

            public GraphDataItem DataItem { get; set; }

            public GraphFieldExecutionContext FieldContext { get; set; }
        }

        private readonly ISchemaPipeline<TSchema, GraphFieldExecutionContext> _fieldExecutionPipeline;
        private readonly TSchema _schema;
        private readonly ResolverIsolationOptions _resolversToIsolate;
        private readonly bool _debugMode;
        private readonly TimeSpan _timeoutMs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteQueryOperationMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="fieldExecutionPipeline">The field execution pipeline.</param>
        public ExecuteQueryOperationMiddleware(TSchema schema, ISchemaPipeline<TSchema, GraphFieldExecutionContext> fieldExecutionPipeline)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _resolversToIsolate = _schema.Configuration.ExecutionOptions.ResolverIsolation;
            _debugMode = _schema.Configuration.ExecutionOptions.DebugMode;
            _timeoutMs = _schema.Configuration.ExecutionOptions.QueryTimeout;
            _fieldExecutionPipeline = Validation.ThrowIfNullOrReturn(fieldExecutionPipeline, nameof(fieldExecutionPipeline));
        }

        /// <summary>
        /// Invokes the asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="next">The next.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        public async Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            context.Metrics?.StartPhase(ApolloExecutionPhase.EXECUTION);
            if (context.IsValid && context.QueryOperation != null)
            {
                await this.ExecuteOperation(context).ConfigureAwait(false);
            }

            await next(context, cancelToken).ConfigureAwait(false);
            context.Metrics?.EndPhase(ApolloExecutionPhase.EXECUTION);
        }

        private async Task ExecuteOperation(GraphQueryExecutionContext context)
        {
            // create a cancelation sourc irrespective of the required timeout for exeucting this operation
            // this allows for indication of why a task was canceled (timeout or other user driven reason)
            // vs just "it was canceled" which allows for tighter error messages in the response.
            var operation = context.QueryOperation;
            var fieldInvocations = new List<FieldPipelineInvocation>();
            var fieldInvocationTasks = new List<Task>();

            // Convert the supplied variable values to usable objects of the type expression
            // of the chosen operation
            var variableResolver = new ResolvedVariableGenerator(_schema, operation);
            var variableData = variableResolver.Resolve(context.OperationRequest.VariableData);
            var cancelSource = new CancellationTokenSource();

            try
            {
                var timeOutTask = Task.Delay(_timeoutMs, cancelSource.Token);

                // Step 0
                // --------------------------
                // Sort the top level queries of this operation such that
                // those contexts that should be isolated execute first and all others
                // run in paralell
                IEnumerable<(IGraphFieldInvocationContext Context, bool ExecuteIsolated)> orderedContextList;
                if (operation.OperationType == GraphCollection.Mutation)
                {
                    // top level mutation operatons must be executed in sequential order
                    // due to potential side effects on the data
                    // https://graphql.github.io/graphql-spec/June2018/#sec-Mutation
                    orderedContextList = operation.FieldContexts
                        .Select(x => (x, true));
                }
                else
                {
                    // with non-mutation queries, order the contexts such that the isolated ones (as determined by
                    // the configuration for this schema) are on top. All contexts are ran in isolation
                    // when in debug mode (always).
                    orderedContextList = operation
                       .FieldContexts
                       .Select(x => (x, _debugMode || _resolversToIsolate.ShouldIsolateFieldSource(x.Field.FieldSource)))
                       .OrderByDescending(x => x.Item2);
                }

                // Step 1
                // --------------------------
                // Begin a field execution pipeline for each top level field
                foreach (var sortedContext in orderedContextList)
                {
                    var path = new SourcePath();
                    path.AddFieldName(sortedContext.Context.Name);

                    object dataSourceValue;

                    // fetch the source data value to use for the field invocation
                    // attempt to retrieve from the master context if it was supplied by the pipeline
                    // invoker, otherwise generate a root source
                    if (!context.DefaultFieldSources.TryRetrieveSource(sortedContext.Context.Field, out dataSourceValue))
                        dataSourceValue = this.GenerateRootSourceData(operation.OperationType);

                    var topLevelDataItem = new GraphDataItem(sortedContext.Context, dataSourceValue, path);

                    var sourceData = new GraphFieldDataSource(dataSourceValue, path, topLevelDataItem);

                    var fieldRequest = new GraphFieldRequest(
                        context.OperationRequest,
                        sortedContext.Context,
                        sourceData,
                        new SourceOrigin(sortedContext.Context.Origin.Location, path),
                        context.Items);

                    var fieldContext = new GraphFieldExecutionContext(
                        context,
                        fieldRequest,
                        variableData,
                        context.DefaultFieldSources);

                    var fieldTask = _fieldExecutionPipeline.InvokeAsync(fieldContext, cancelSource.Token);

                    var pipelineInvocation = new FieldPipelineInvocation()
                    {
                        Task = fieldTask,
                        DataItem = topLevelDataItem,
                        FieldContext = fieldContext,
                    };

                    fieldInvocations.Add(pipelineInvocation);
                    fieldInvocationTasks.Add(fieldTask);

                    if (_debugMode)
                    {
                        await fieldTask.ConfigureAwait(false);
                    }
                    else if (sortedContext.ExecuteIsolated)
                    {
                        // await the isolated task to prevent any potential paralellization
                        // by the task system but not in such a way that a faulted task would
                        // throw an exception. Allow the reslts (exceptions included) to be
                        // captured on the task and handled by the rest of the middleware operation
                        //
                        // while awaiting an isolated task the query timeout may expire
                        // exit and stop if so
                        await Task.WhenAny(fieldTask, timeOutTask).ConfigureAwait(false);
                    }

                    if (timeOutTask.IsCompleted)
                        break;
                }

                // Step 2
                // -----------------------------------------
                bool isTimedOut;

                if (timeOutTask.IsCompleted)
                {
                    // timeout could have occured during the completion of any isolated task
                    // if it did dont try to wait for an paralell tasks to finish
                    isTimedOut = true;
                }
                else
                {
                    // await all outstanding task and hope they finish before the timer
                    var fieldPipelineTasksWrapper = Task.WhenAll(fieldInvocationTasks);
                    var completedTask = await Task.WhenAny(fieldPipelineTasksWrapper, timeOutTask).ConfigureAwait(false);

                    isTimedOut = completedTask == timeOutTask;
                }

                var cancelationWasRequested = cancelSource.IsCancellationRequested;
                if (!isTimedOut)
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

                    // Consider that the task(s) may have faulted or been canceled causing them to complete incorrectly.
                    // "re-await" a single failure or aggregate many failues so that any exceptions/cancellation are rethrown correctly.
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
                else
                {
                    // when the timeout finishes first, process the cancel token in case any outstanding tasks are running
                    // helps in cases where the timeout finished first but any of the field resolutions are perhaps stuck open
                    // instruct all outstanding tasks to clean them selves up at the earlest possible point
                    if (!cancelationWasRequested)
                        cancelSource.Cancel();
                }

                if (cancelationWasRequested)
                {
                    context.Messages.Critical("The execution was canceled prior to completion of the requested query.", Constants.ErrorCodes.OPERATION_CANCELED);
                }
                else if (isTimedOut)
                {
                    context.Messages.Critical($"The execution timed out prior to completion of the requested query. (Total Time: {_timeoutMs}ms)", Constants.ErrorCodes.OPERATION_CANCELED);
                }
            }
            finally
            {
                cancelSource.Dispose();
            }
        }

        /// <summary>
        /// Generates a "top level" source data representing the operation root under which a field
        /// context should be executed.
        /// </summary>
        /// <param name="operationType">Type of the operation.</param>
        /// <returns>System.Object.</returns>
        private object GenerateRootSourceData(GraphCollection operationType)
        {
            if (_schema.OperationTypes.TryGetValue(operationType, out var rootOperation))
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