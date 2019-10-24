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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Execution.Metrics;
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
        private readonly bool _awaitEachTask;
        private readonly TimeSpan _timeoutMs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteQueryOperationMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="fieldExecutionPipeline">The field execution pipeline.</param>
        public ExecuteQueryOperationMiddleware(TSchema schema, ISchemaPipeline<TSchema, GraphFieldExecutionContext> fieldExecutionPipeline)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _awaitEachTask = _schema.Configuration.ExecutionOptions.AwaitEachRequestedField;
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

            // Convert the supplied variable values to usable objects of the type expression
            // of the chosen operation
            var variableResolver = new ResolvedVariableGenerator(_schema, operation);
            var variableData = variableResolver.Resolve(context.Request.VariableData);
            var cancelSource = new CancellationTokenSource();
            try
            {
                // begin a field execution pipeline for each top level field
                foreach (var invocationContext in operation.FieldContexts)
                {
                    var path = new SourcePath();
                    path.AddFieldName(invocationContext.Name);

                    var dataSourceValue = this.GenerateRootSourceData(operation.OperationType);
                    var topLevelDataItem = new GraphDataItem(invocationContext, dataSourceValue, path);

                    var sourceData = new GraphFieldDataSource(dataSourceValue, path, topLevelDataItem);

                    var fieldRequest = new GraphFieldRequest(
                        invocationContext,
                        sourceData,
                        new SourceOrigin(invocationContext.Origin.Location, path));

                    var fieldContext = new GraphFieldExecutionContext(
                        context,
                        fieldRequest,
                        variableData);

                    var fieldTask = _fieldExecutionPipeline.InvokeAsync(fieldContext, cancelSource.Token);

                    var pipelineInvocation = new FieldPipelineInvocation()
                    {
                        Task = fieldTask,
                        DataItem = topLevelDataItem,
                        FieldContext = fieldContext,
                    };

                    fieldInvocations.Add(pipelineInvocation);

                    // top level mutation operatons must be executed in sequential order
                    // https://graphql.github.io/graphql-spec/June2018/#sec-Mutation
                    if (_awaitEachTask || operation.OperationType == GraphCollection.Mutation)
                        await fieldTask.ConfigureAwait(false);
                }

                // await all the outstanding tasks or a configured timeout
                var fieldPipelineTasksWrapper = Task.WhenAll(fieldInvocations.Select(x => x.Task));
                var timeOutTask = Task.Delay(_timeoutMs, cancelSource.Token);

                var completedTask = await Task.WhenAny(fieldPipelineTasksWrapper, timeOutTask).ConfigureAwait(false);

                var isTimedOut = completedTask == timeOutTask;
                var cancelationWasRequested = cancelSource.IsCancellationRequested;

                if (!isTimedOut)
                {
                    // Field resolutions completed within the timeout period.
                    // Consider that the task may have faulted or been canceled causing them to complete incorrectly.
                    // "re-await" so that any exceptions/cancellation are rethrown correctly.
                    // and not aggregated under the `WhenAll/WhenAny` task from above
                    // https://stackoverflow.com/questions/4238345/asynchronously-wait-for-taskt-to-complete-with-timeout
                    foreach (var invocation in fieldInvocations)
                    {
                        await invocation.Task.ConfigureAwait(false);

                        // load the reslts of each field (in order) to the context
                        // for further processing
                        context.FieldResults.Add(invocation.DataItem);
                        context.Messages.AddRange(invocation.FieldContext.Messages);
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