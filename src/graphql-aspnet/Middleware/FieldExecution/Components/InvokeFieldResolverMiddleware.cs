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
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.RulesEngine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A middleware component to create a <see cref="GraphController" /> and invoke an action method.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component exists for.</typeparam>
    public class InvokeFieldResolverMiddleware<TSchema> : IFieldExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly TSchema _schema;
        private readonly FieldCompletionRuleProcessor _completionProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeFieldResolverMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema targeted by this middleware component.</param>
        public InvokeFieldResolverMiddleware(
            TSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _completionProcessor = new FieldCompletionRuleProcessor();
        }

        /// <inheritdoc />
        public async Task InvokeAsync(GraphFieldExecutionContext context, GraphMiddlewareInvocationDelegate<GraphFieldExecutionContext> next, CancellationToken cancelToken = default)
        {
            // create a set of validation contexts for every incoming source graph item
            // to capture and validate every item regardless of it being successfully resolved or failed
            var validationContexts = new List<FieldValidationContext>(context.Request.Data.Items.Count);
            for (var i = 0; i < context.Request.Data.Items.Count; i++)
            {
                var dataItem = context.Request.Data.Items[i];
                var validationContext = new FieldValidationContext(
                    _schema,
                    dataItem,
                    context.Messages);

                validationContexts.Add(validationContext);
            }

            // begin profiling of this single field of data
            context.Metrics?.BeginFieldResolution(context);
            var continueExecution = true;
            if (context.IsValid)
                continueExecution = await this.ExecuteContext(context, cancelToken).ConfigureAwait(false);

            if (!continueExecution)
            {
                context.Cancel();
                for (var i = 0; i < context.Request.Data.Items.Count; i++)
                {
                    context.Request.Data.Items[i].Cancel();
                }
            }

            // validate the resolution of the field in whatever manner is necessary
            // for its current state
            _completionProcessor.Execute(validationContexts);

            // end profiling of this single field of data
            context.Metrics?.EndFieldResolution(context);

            await next(context, cancelToken).ConfigureAwait(false);

            // validate the final result after all downstream middleware execute
            // in the standard pipeline this generally means all child fields have resolved
            var validationProcessor = new FieldValidationRuleProcessor();
            validationProcessor.Execute(validationContexts);
        }

        private async Task<bool> ExecuteContext(GraphFieldExecutionContext context, CancellationToken cancelToken = default)
        {
            // Step 1: Build a collection of arguments from the supplied context that will
            //         be supplied to teh resolver
            var executionArguments = context
                .InvocationContext
                .Arguments
                .Merge(context.VariableData)
                .ForContext(context);

            var resolutionContext = new FieldResolutionContext(
                _schema,
                context,
                context.Request,
                executionArguments,
                context.User);

            // Step 2: Resolve the field
            context.Logger?.FieldResolutionStarted(resolutionContext);

            var task = context.Field?.Resolver?.Resolve(resolutionContext, cancelToken);
            await task.ConfigureAwait(false);
            context.Messages.AddRange(resolutionContext.Messages);

            await this.CompletePostFieldResolutionWork(context, resolutionContext, cancelToken);

            context.Logger?.FieldResolutionCompleted(resolutionContext);

            this.AssignResults(context, resolutionContext);

            return !resolutionContext.IsCancelled;
        }

        /// <summary>
        /// A method that executes immediately after the field resolver completes its
        /// operation. By default this method is used to invoke a fields post resolver but can be
        /// extended as necessary.
        /// </summary>
        /// <param name="fieldExecutionContext">The field execution context governing the
        /// request.</param>
        /// <param name="fieldResolutionContext">The specific field resolution context
        /// that was just resolved.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task CompletePostFieldResolutionWork(
            GraphFieldExecutionContext fieldExecutionContext,
            FieldResolutionContext fieldResolutionContext,
            CancellationToken cancelToken)
        {
            // execute the post processor assigned via the query document if one exists
            if (!fieldResolutionContext.IsCancelled)
            {
                if (fieldExecutionContext.InvocationContext.FieldDocumentPart.PostResolver != null)
                    await fieldExecutionContext.InvocationContext.FieldDocumentPart.PostResolver(fieldResolutionContext, cancelToken);
            }
        }

        /// <summary>
        /// Assigns the results of resolving the field to the items on the execution context.
        /// </summary>
        /// <param name="executionContext">The field execution context governing the field resolution.</param>
        /// <param name="resolutionContext">The resolution context that just completed processing
        /// a field resolver.</param>
        private void AssignResults(GraphFieldExecutionContext executionContext, FieldResolutionContext resolutionContext)
        {
            // transfer the result to the execution context
            // then deteremine what (if any) data items can be updated from its value
            executionContext.Result = resolutionContext.Result;

            if (executionContext.Field.Mode == FieldResolutionMode.PerSourceItem)
            {
                if (executionContext.Request.Data.Items.Count == 1)
                {
                    var item = executionContext.Request.Data.Items[0];
                    executionContext.ResolvedSourceItems.Add(item);
                    item.AssignResult(resolutionContext.Result);
                    return;
                }

                throw new GraphExecutionException(
                    $"When attempting to resolve the field '{executionContext.Field.Route.Path}' an unexpected error occured and the request was teriminated.",
                    executionContext.Request.Origin,
                    new InvalidOperationException(
                        $"The field '{executionContext.Field.Route.Parent}' has a resolution mode of '{nameof(FieldResolutionMode.PerSourceItem)}' " +
                        $"but the execution context contains {executionContext.Request.Data.Items.Count} source items. The runtime is unable to determine which " +
                        "item to assign the resultant value to."));
            }
            else if (executionContext.Field.Mode == FieldResolutionMode.Batch)
            {
                var batchProcessor = new BatchResultProcessor(
                    executionContext.Field,
                    executionContext.Request.Data.Items,
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