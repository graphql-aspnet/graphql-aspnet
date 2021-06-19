// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.FieldExecution.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.ValidationRules;

    /// <summary>
    /// A middleware component to create a <see cref="GraphController" /> and invoke an action method.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component exists for.</typeparam>
    public class InvokeFieldResolverMiddleware<TSchema> : IGraphFieldExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly TSchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeFieldResolverMiddleware{TSchema}"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public InvokeFieldResolverMiddleware(TSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Invoke the action item as an asyncronous operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="next">The next.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        public async Task InvokeAsync(GraphFieldExecutionContext context, GraphMiddlewareInvocationDelegate<GraphFieldExecutionContext> next, CancellationToken cancelToken = default)
        {
            // create a set of validation contexts for every incoming source graph item
            // to capture and validate every item regardless of it being successfully resolved or failed
            var validationContexts = new List<FieldValidationContext>(context.Request.DataSource.Items.Count);
            foreach (var dataItem in context.Request.DataSource.Items)
            {
                var validationContext = new FieldValidationContext(_schema, dataItem, context.Messages);
                validationContexts.Add(validationContext);
            }

            // begin profiling of this single field of data
            context.Metrics?.BeginFieldResolution(context);
            bool fieldShouldBeCanceled = false;
            if (context.IsValid)
            {
                // build a collection of invokable parameters from the supplied context
                var executionArguments = context
                    .InvocationContext
                    .Arguments
                    .Merge(context.VariableData)
                    .WithSourceData(context.Request.DataSource.Value);

                // resolve the field
                var resolutionContext = new FieldResolutionContext(context, context.Request, executionArguments);

                context.Logger?.FieldResolutionStarted(resolutionContext);

                var task = context.Field?.Resolver?.Resolve(resolutionContext, cancelToken);

                await task.ConfigureAwait(false);
                context.Messages.AddRange(resolutionContext.Messages);

                this.AssignResults(context, resolutionContext);
                fieldShouldBeCanceled = resolutionContext.IsCancelled;

                context.Logger?.FieldResolutionCompleted(resolutionContext);
            }

            if (fieldShouldBeCanceled)
            {
                context.Cancel();
                context.Request.DataSource.Items.ForEach(x => x.Cancel());
            }

            // validate the resolution of the field in whatever manner that means for its current state
            var completionProcessor = new FieldCompletionRuleProcessor();
            completionProcessor.Execute(validationContexts);

            // end profiling of this single field of data
            context.Metrics?.EndFieldResolution(context);

            await next(context, cancelToken).ConfigureAwait(false);

            // validate the final result after all downstream middleware execute
            // in the standard pipeline this generally means all child fields have resolved
            var validationProcessor = new FieldValidationRuleProcessor();
            validationProcessor.Execute(validationContexts);
        }

        /// <summary>
        /// Assigns the results of resolving the field to the items on the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="resolutionContext">The resolution context.</param>
        private void AssignResults(GraphFieldExecutionContext executionContext, FieldResolutionContext resolutionContext)
        {
            // transfer the result to the execution context
            // then deteremine what (if any) data items can be updated from its value
            executionContext.Result = resolutionContext.Result;

            if (executionContext.Field.Mode == FieldResolutionMode.PerSourceItem)
            {
                if (executionContext.Request.DataSource.Items.Count == 1)
                {
                    var item = executionContext.Request.DataSource.Items[0];
                    executionContext.ResolvedSourceItems.Add(item);
                    item.AssignResult(resolutionContext.Result);
                    return;
                }

                throw new GraphExecutionException(
                    $"When attempting to resolve the field '{executionContext.Field.Route.Path}' an unexpected error occured and the request was teriminated.",
                    executionContext.Request.Origin,
                    new InvalidOperationException(
                        $"The field '{executionContext.Field.Route.Parent}' has a resolution mode of '{nameof(FieldResolutionMode.PerSourceItem)}' " +
                        $"but the execution context contains {executionContext.Request.DataSource.Items.Count} source items. The runtime is unable to determine which " +
                        "item to assign the resultant value to."));
            }
            else if (executionContext.Field.Mode == FieldResolutionMode.Batch)
            {
                var batchProcessor = new BatchResultProcessor(
                    executionContext.Field,
                    executionContext.Request.DataSource.Items,
                    executionContext.Request.Origin);

                var itemsWithAssignedData = batchProcessor.Resolve(executionContext.Result);
                executionContext.ResolvedSourceItems.AddRange(itemsWithAssignedData);
                executionContext.Messages.AddRange(batchProcessor.Messages);
                return;
            }

            throw new ArgumentOutOfRangeException(
                nameof(executionContext.Field.Mode),
                $"The execution mode for field '{executionContext.Field.Route.Path}' cannot be resolved " +
                $"by {nameof(InvokeFieldResolverMiddleware<TSchema>)}. (Mode: {executionContext.Field.Mode.ToString()})");
        }
    }
}