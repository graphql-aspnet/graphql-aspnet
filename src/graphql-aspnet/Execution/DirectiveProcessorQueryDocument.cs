// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.InputArguments;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Executes any supplied directives on a query document against those
    /// document parts where they are applied.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema to work with.</typeparam>
    internal sealed class DirectiveProcessorQueryDocument<TSchema>
        where TSchema : class, ISchema
    {
        private readonly TSchema _schema;
        private readonly GraphQueryExecutionContext _queryContext;
        private IGraphEventLogger _eventLogger;
        private ISchemaPipeline<TSchema, GraphDirectiveExecutionContext> _directivePipeline;
        private IOperationDocumentPart _operation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveProcessorQueryDocument{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema in focus while processing directives.</param>
        /// <param name="queryContext">The query context.</param>
        public DirectiveProcessorQueryDocument(
            TSchema schema,
            GraphQueryExecutionContext queryContext)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _queryContext = Validation.ThrowIfNullOrReturn(queryContext, nameof(queryContext));
        }

        /// <summary>
        /// Scans and applies the directives found on the operation (and any referenced named fragments)
        /// to the parts where they are defined.
        /// </summary>
        /// <param name="operation">The operation in which the directives shound be applied.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        public async Task<int> ApplyDirectives(
            IOperationDocumentPart operation,
            CancellationToken cancelToken = default)
        {
            _operation = Validation.ThrowIfNullOrReturn(operation, nameof(operation));

            var directivesToExecute = new List<IDirectiveDocumentPart>(_operation.AllDirectives.Count + _operation.FragmentSpreads.Count);
            directivesToExecute.AddRange(_operation.AllDirectives);

            // append the directive invaocations on any referenced named fragments
            directivesToExecute.AddRange(_operation
                .FragmentSpreads
                .Select(x => x.Fragment)
                .Distinct()
                .SelectMany(x => x.AllDirectives));

            // no directives? just get out
            if (directivesToExecute.Count == 0)
                return 0;

            _eventLogger = _queryContext.ServiceProvider.GetService<IGraphEventLogger>();
            _directivePipeline = _queryContext.ServiceProvider.GetService<ISchemaPipeline<TSchema, GraphDirectiveExecutionContext>>();
            if (_directivePipeline == null)
            {
                throw new GraphExecutionException(
                    "Unable to process document directives. " +
                    $"No directive processing pipeline was registered in the " +
                    $"service provider for the target schema '{_schema.Name}'.");
            }

            var totalApplied = 0;
            foreach (var directiveDocumentPart in directivesToExecute)
            {
                var targetPart = directiveDocumentPart.Parent;
                await this.ApplyDirectiveToItem(targetPart, directiveDocumentPart, cancelToken);
                totalApplied++;
            }

            return totalApplied;
        }

        private async Task ApplyDirectiveToItem(
            IDocumentPart targetDocumentPart,
            IDirectiveDocumentPart directiveDocumentPart,
            CancellationToken cancelToken)
        {
            var targetDirective = directiveDocumentPart.GraphType as IDirective;
            if (targetDirective == null)
            {
                var directiveName = directiveDocumentPart.DirectiveName ?? "-unknown-";
                var failureMessage =
                    $"Document Directive Invocation Failure. " +
                    $"The directive type named '{directiveName}' " +
                    $"does not represent a valid directive on the target schema. (Schema: {_schema.Name})";

                throw new GraphExecutionException(
                    failureMessage,
                    targetDocumentPart.Node.Location.AsOrigin());
            }

            var inputArgs = this.GatherInputArguments(targetDirective, directiveDocumentPart);

            var parentRequest = _queryContext.OperationRequest;

            var invocationContext = new DirectiveInvocationContext(
                targetDirective,
                directiveDocumentPart.Location,
                targetDocumentPart.Node.Location.AsOrigin(),
                inputArgs);

            var request = new GraphDirectiveRequest(
                invocationContext,
                DirectiveInvocationPhase.QueryDocumentExecution,
                targetDocumentPart);

            var context = new GraphDirectiveExecutionContext(
                _schema,
                _queryContext.ServiceProvider,
                parentRequest,
                request,
                _queryContext.Metrics,
                _eventLogger,
                items: request.Items);

            Exception causalException = null;

            try
            {
                await _directivePipeline.InvokeAsync(context, cancelToken);
            }
            catch (Exception ex)
            {
                // SUPER FAIL!!!
                context.Cancel();
                causalException = ex;
            }

            if (context.IsCancelled || !context.Messages.IsSucessful)
            {
                // attempt to discover the reason for the failure if its contained within the
                // executed context
                if (causalException == null)
                {
                    // when lots of failures are indicated
                    // nest them into each other before throwing
                    foreach (var message in context.Messages.Where(x => x.Severity.IsCritical()))
                    {
                        // when an actual exception was encountered
                        // use it as the causal exception
                        if (message.Exception != null)
                        {
                            causalException = message.Exception;
                            break;
                        }

                        // otherwise chain together any failure messages
                        var errorMessage = message.Message;
                        if (!string.IsNullOrWhiteSpace(message.Code))
                            errorMessage = message.Code + " : " + errorMessage;

                        causalException = new GraphExecutionException(
                            errorMessage,
                            targetDocumentPart.Node.Location.AsOrigin(),
                            causalException);
                    }

                    if (causalException == null)
                    {
                        // out of options, cant figure out the issue
                        // just declare a general failure   ¯\_(ツ)_/¯
                        causalException = new GraphExecutionException(
                            $"An Unknown error occured while applying a directive " +
                            $"to the query document (Directive: '{targetDirective.Name}')",
                            targetDocumentPart.Node.Location.AsOrigin());
                    }
                }

                throw new GraphExecutionException(
                    $"An exception occured applying the execution directive '{targetDirective.Name}' to the query document. " +
                    $"See inner exception(s) for details.",
                    targetDocumentPart.Node.Location.AsOrigin(),
                    innerException: causalException);
            }

            // _eventLogger?.TypeSystemDirectiveApplied<TSchema>(targetDirective, item);
        }

        private IInputArgumentCollection GatherInputArguments(
            IDirective targetDirective,
            IDirectiveDocumentPart directivePart)
        {
            var argGenerator = new ArgumentGenerator(_schema, directivePart.Arguments);

            var collection = new InputArgumentCollection();
            foreach (var directiveArg in targetDirective.Arguments)
            {
                var argResult = argGenerator.CreateInputArgument(directiveArg);
                if (argResult.IsValid)
                    collection.Add(new InputArgument(directiveArg, argResult.Argument));
                else
                    _queryContext.Messages.Add(argResult.Message);
            }

            return collection;
        }
    }
}