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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A middleware component that, when a result exists on the context, invokes the next set
    /// of downstream child fields decended from the active field.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middl;eware exists for.</typeparam>
    public class ProcessChildFieldsMiddleware<TSchema> : IGraphFieldExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly ISchemaPipeline<TSchema, GraphFieldExecutionContext> _fieldExecutionPipeline;
        private readonly TSchema _schema;
        private readonly bool _awaitEachPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessChildFieldsMiddleware{TSchema}"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="fieldExecutionPipeline">The field execution pipeline.</param>
        public ProcessChildFieldsMiddleware(TSchema schema, ISchemaPipeline<TSchema, GraphFieldExecutionContext> fieldExecutionPipeline)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _awaitEachPipeline = schema.Configuration.ExecutionOptions.AwaitEachRequestedField;
            _fieldExecutionPipeline = Validation.ThrowIfNullOrReturn(fieldExecutionPipeline, nameof(fieldExecutionPipeline));
        }

        /// <summary>
        /// Invokes this middleware component allowing it to perform its work against the supplied context.
        /// </summary>
        /// <param name="context">The context containing the request passed through the pipeline.</param>
        /// <param name="next">The delegate pointing to the next piece of middleware to be invoked.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        public async Task InvokeAsync(GraphFieldExecutionContext context, GraphMiddlewareInvocationDelegate<GraphFieldExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.IsValid && context.Result != null && !context.IsCancelled)
                await this.ProcessDownStreamFieldContexts(context, cancelToken).ConfigureAwait(false);

            await next(context, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// For any resolved, non-leaf items assigned to the result, pass each through the resolution pipeline
        /// and await their individual results.
        /// </summary>
        /// <param name="context">The "parent" context supplying data for downstream reslts.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        private async Task ProcessDownStreamFieldContexts(GraphFieldExecutionContext context, CancellationToken cancelToken)
        {
            if (context.InvocationContext.ChildContexts.Count == 0)
                return;

            // items resolved on this active context become the source for any downstream fields
            //  ---
            // can never extract child fields from a null value (even if its valid for the item)
            // or one that isnt read for it
            List<GraphDataItem> allSourceItems = context
                .ResolvedSourceItems
                .SelectMany(x => x.FlattenListItemTree())
                .Where(x => x.ResultData != null && x.Status == FieldItemResolutionStatus.NeedsChildResolution)
                .ToList();

            if (allSourceItems.Count == 0)
                return;

            // find a reference to the graph type for the field
            var graphType = _schema.KnownTypes.FindGraphType(context.Field.TypeExpression.TypeName);

            // theoretically it can't not be found, but you never know
            if (graphType == null)
            {
                var msg = $"Internal Server Error. When processing the results of '{context.Field.Route.Path}' no graph type on the target schema " +
                    $"could be found for the type name '{context.Field.TypeExpression.TypeName}'. " +
                    $"Unable to process the {allSourceItems.Count} item(s) generated.";

                context.Messages.Add(
                    GraphMessageSeverity.Critical,
                    Constants.ErrorCodes.EXECUTION_ERROR,
                    msg,
                    context.Request.Origin);

                context.Cancel();
                return;
            }

            var pipelines = new List<Task>();

            // Step 0
            // -----------------------------------------------------------------------
            // create a lookup of source items by concrete type known to the schema, for easy seperation to the individual
            // downstream child contexts
            var sourceItemLookup = this.MapExpectedConcreteTypeFromSourceItem(allSourceItems, graphType);

            foreach (var childInvocationContext in context.InvocationContext.ChildContexts)
            {
                // Step 1
                // ----------------------------
                // figure out which child items need to be processed through it
                IEnumerable<GraphDataItem> sourceItemsToInclude;
                if (childInvocationContext.ExpectedSourceType == null)
                {
                    sourceItemsToInclude = allSourceItems;
                }
                else
                {
                    // if no children match the required type of the children present, then skip it
                    // this can happen quite often in the case of a union or an interface where multiple invocation contexts
                    // are added to a plan for the same child field in case a parent returns a member of the union or an
                    // implementer of the interface
                    if (!sourceItemLookup.ContainsKey(childInvocationContext.ExpectedSourceType))
                        continue;

                    sourceItemsToInclude = sourceItemLookup[childInvocationContext.ExpectedSourceType];
                }

                // Step 1B
                // For any source items replace any virtual objects with defaults found
                // on the context for the field in question
                if (context.DefaultFieldSources.TryRetrieveSource(childInvocationContext.Field, out var defaultSource))
                {
                    sourceItemsToInclude = sourceItemsToInclude.Select((currentValue) =>
                    {
                        if (currentValue.ResultData is VirtualResolvedObject)
                            currentValue.AssignResult(defaultSource);

                        return currentValue;
                    });
                }

                // Step 2
                // ----------------------------
                // when the invocation is as a batch, create one execution context for all children
                // when its "per source" create a context for each child individually
                IEnumerable<GraphFieldExecutionContext> childContexts = this.CreateChildExecutionContexts(
                    context,
                    childInvocationContext,
                    sourceItemsToInclude);

                // Step 3
                // --------------------
                // Fire off the contexts through the pipeline
                foreach (var childContext in childContexts)
                {
                    var task = _fieldExecutionPipeline.InvokeAsync(childContext, cancelToken)
                        .ContinueWith(invokeTask =>
                        {
                            this.CaptureChildFieldExecutionResults(context, childContext, invokeTask);
                        });

                    pipelines.Add(task);
                    if (_awaitEachPipeline)
                        await task.ConfigureAwait(false);
                }
            }

            // wait for every pipeline to finish
            await Task.WhenAll(pipelines).ConfigureAwait(false);

            // reawait to allow for unwrapping and throwing of internal exceptions
            if (!_awaitEachPipeline)
            {
                foreach (var task in pipelines.Where(x => x.IsFaulted))
                {
                    await task.ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Inspects both the childcontext and the invoked task to determine
        /// if any messages were found or any unhandled exceptions were thrown then
        /// captures the results into the parent context for upstream processing or
        /// reporting.
        /// </summary>
        /// <param name="parentContext">The parent context that invoked the child.</param>
        /// <param name="childContext">The child context that was invoked.</param>
        /// <param name="childExecutionTask">The actual task representing the child
        /// pipeline.</param>
        private void CaptureChildFieldExecutionResults(
            GraphFieldExecutionContext parentContext,
            GraphFieldExecutionContext childContext,
            Task childExecutionTask)
        {
            // capture any messages that the child context may have internally reported
            parentContext.Messages.AddRange(childContext.Messages);

            // inspect the task for faulting and capture any exceptions as critical errors
            if (childExecutionTask.IsFaulted)
            {
                var exception = childExecutionTask.UnwrapException();
                exception = exception ?? new Exception(
                    "Unknown Error. The child field execution pipeline indicated a fault but did not " +
                    "provide a reason for the failure.");

                parentContext.Messages.Critical(
                    $"Processing field '{childContext.Field.Name}' of '{parentContext.Field.Route}' resulted in a critical failure. See exception for details.",
                    Constants.ErrorCodes.EXECUTION_ERROR,
                    childContext.InvocationContext.Origin,
                    exception);
            }
        }

        private IDictionary<Type, List<GraphDataItem>> MapExpectedConcreteTypeFromSourceItem(
                List<GraphDataItem> allSourceItems,
                IGraphType expectedGraphType)
        {
            var dic = new Dictionary<Type, List<GraphDataItem>>();

            // when the target graph type is not "mapable", generate a dictionary by exact type matching
            switch (expectedGraphType.Kind)
            {
                case TypeKind.NONE:
                case TypeKind.SCALAR:
                case TypeKind.ENUM:
                case TypeKind.INPUT_OBJECT:
                    foreach (var item in allSourceItems)
                    {
                        if (!dic.ContainsKey(item.GetType()))
                            dic.Add(item.GetType(), new List<GraphDataItem>());

                        dic[item.GetType()].Add(item);
                    }

                    return dic;
            }

            // find the applied concrete type, given the expected graph type, for each source item
            // fail if no exact match can be found for any given source data item.
            for (var i = 0; i < allSourceItems.Count; i++)
            {
                var dataItem = allSourceItems[i];
                var dataResult = dataItem.ResultData;
                if (dataResult == null)
                    continue;

                var sourceItemType = dataItem.ResultData.GetType();
                var result = _schema.KnownTypes.AnalyzeRuntimeConcreteType(expectedGraphType, sourceItemType);
                if (result.ExactMatchFound)
                {
                    if (!dic.ContainsKey(result.FoundTypes[0]))
                        dic.Add(result.FoundTypes[0], new List<GraphDataItem>());

                    dic[result.FoundTypes[0]].Add(dataItem);
                }

                // validation rules 6.4.3 will always pick up (and kill)
                // any un matchable or un process-able results. we don't need
                // to check failures here
            }

            return dic;
        }

        /// <summary>
        /// Using the child context being invoked, this method creates the execution contexts in a manner
        /// that is expected by the invocation context be that 1 per each item, or 1 for a collective set of items being batched.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="childInvocationContext">The child invocation context.</param>
        /// <param name="sourceItemsToInclude">The source items to include.</param>
        /// <returns>IEnumerable&lt;GraphFieldExecutionContext&gt;.</returns>
        private IEnumerable<GraphFieldExecutionContext> CreateChildExecutionContexts(
            GraphFieldExecutionContext context,
            IGraphFieldInvocationContext childInvocationContext,
            IEnumerable<GraphDataItem> sourceItemsToInclude)
        {
            if (childInvocationContext.Field.Mode == FieldResolutionMode.PerSourceItem)
            {
                foreach (var sourceItem in sourceItemsToInclude)
                {
                    var child = sourceItem.AddChildField(childInvocationContext);

                    var dataSource = new GraphFieldDataSource(sourceItem.ResultData, child.Origin.Path, child);
                    var request = new GraphFieldRequest(childInvocationContext, dataSource, child.Origin, context.Request.Items);
                    yield return new GraphFieldExecutionContext(
                        context,
                        request,
                        context.VariableData,
                        context.DefaultFieldSources);
                }
            }
            else if (childInvocationContext.Field.Mode == FieldResolutionMode.Batch)
            {
                // remove any potential indexers from the path to this batch operation
                // in general this will be acted on a collection of items, attempt to remove
                // the first found instance of an indexer in the chain to indicate the path to the batch
                //
                // items may be declared as:        Top.Parent[0].BatchField, Top.Parent[1].BatchField
                // alter the canonical path to be:  Top.Parent.BatchField
                var fieldPath = sourceItemsToInclude.First().Origin.Path.Clone();
                while (fieldPath.IsIndexedItem)
                    fieldPath = fieldPath.MakeParent();

                fieldPath.AddFieldName(childInvocationContext.Field.Name);
                var batchOrigin = new SourceOrigin(context.Request.Origin.Location, fieldPath);

                // create a list to house the raw source data being passed for the batch
                // this is the IEnumerable<T> required as an input to any batch resolver
                var sourceArgumentType = childInvocationContext.Field.Arguments.SourceDataArgument?.ObjectType ?? typeof(object);
                var sourceListType = typeof(List<>).MakeGenericType(sourceArgumentType);
                var sourceDataList = InstanceFactory.CreateInstance(sourceListType) as IList;

                // create a list of all the GraphDataItems representing the field
                // being resolved per input item
                var sourceItemList = new List<GraphDataItem>();

                foreach (var item in sourceItemsToInclude)
                {
                    var childField = item.AddChildField(childInvocationContext);
                    sourceDataList.Add(item.ResultData);
                    sourceItemList.Add(childField);
                }

                var dataSource = new GraphFieldDataSource(
                    sourceDataList,
                    fieldPath,
                    sourceItemList);

                var request = new GraphFieldRequest(childInvocationContext, dataSource, batchOrigin, context.Request.Items);
                yield return new GraphFieldExecutionContext(
                    context,
                    request,
                    context.VariableData,
                    context.DefaultFieldSources);
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    nameof(childInvocationContext.Field.Mode),
                    $"The execution mode for field '{childInvocationContext.Field.Route.Path}' cannot be processed " +
                    $"by {nameof(ProcessChildFieldsMiddleware<TSchema>)}. (Mode: {childInvocationContext.Field.Mode.ToString()})");
            }
        }
    }
}