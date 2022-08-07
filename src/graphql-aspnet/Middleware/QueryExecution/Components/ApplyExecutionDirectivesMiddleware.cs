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
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.PlanGeneration.InputArguments;
    using GraphQL.AspNet.Variables;

    /// <summary>
    /// For the chosen operation to execute, applies any directives to elements and fragments
    /// contained in or referenced by said operation.
    /// </summary>
    /// <typeparam name="TSchema">The schema targeted by this middleware component.</typeparam>
    internal class ApplyExecutionDirectivesMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly TSchema _schema;
        private readonly ISchemaPipeline<TSchema, GraphDirectiveExecutionContext> _directivePipeline;
        private readonly IGraphQueryDocumentGenerator<TSchema> _documentGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyExecutionDirectivesMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema being executed against.</param>
        /// <param name="directivePipeline">The directive pipeline to process applied directives
        /// on.</param>
        /// <param name="documentGenerator">The document generator used to revalidate
        /// the doc after directive execution.</param>
        public ApplyExecutionDirectivesMiddleware(
            TSchema schema,
            ISchemaPipeline<TSchema, GraphDirectiveExecutionContext> directivePipeline,
            IGraphQueryDocumentGenerator<TSchema> documentGenerator)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _directivePipeline = Validation.ThrowIfNullOrReturn(directivePipeline, nameof(directivePipeline));
            _documentGenerator = Validation.ThrowIfNullOrReturn(documentGenerator, nameof(documentGenerator));
        }

        /// <inheritdoc />
        public async Task InvokeAsync(
            GraphQueryExecutionContext context,
            GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next,
            CancellationToken cancelToken = default)
        {
            if (context.IsValid && context.QueryPlan == null && context.Operation != null)
            {
                if (context.Operation.AllDirectives.Count > 0)
                {
                    // when there are directives to apply, start the exectuion phase
                    // early
                    context.Metrics?.StartPhase(ApolloExecutionPhase.EXECUTION);

                    var totalDirectivesApplied = await this.ApplyDirectives(
                        context,
                        context.Operation,
                        cancelToken);

                    if (totalDirectivesApplied > 0)
                    {
                        // revalidate since user supplied directives
                        // may have altered the document structure
                        _documentGenerator.ValidateDocument(context.QueryDocument);
                        if (!context.QueryDocument.Messages.IsSucessful)
                        {
                            context.Messages.AddRange(context.QueryDocument.Messages);
                            context.Cancel();
                        }
                    }
                }
            }

            await next(context, cancelToken).ConfigureAwait(false);
        }

        private async Task<int> ApplyDirectives(
            GraphQueryExecutionContext context,
            IOperationDocumentPart operation,
            CancellationToken cancelToken)
        {
            var directivesToExecute = new List<IDirectiveDocumentPart>(operation.AllDirectives.Count + operation.FragmentSpreads.Count);
            directivesToExecute.AddRange(operation.AllDirectives);

            // append the directive invaocations on any referenced named fragments
            directivesToExecute.AddRange(operation
                .FragmentSpreads
                .Select(x => x.Fragment)
                .Distinct()
                .SelectMany(x => x.AllDirectives));

            // no directives? just get out
            if (directivesToExecute.Count == 0)
                return 0;

            // Directive order matters we can only execute one at a time
            // to ensure predicatable execution order
            // https://spec.graphql.org/October2021/#sec-Language.Directives
            var totalApplied = 0;
            foreach (var directiveDocumentPart in directivesToExecute)
            {
                var targetPart = directiveDocumentPart.Parent;
                await this.ApplyDirectiveToItem(
                    context,
                    targetPart,
                    directiveDocumentPart,
                    cancelToken);

                totalApplied++;
            }

            return totalApplied;
        }

        private async Task ApplyDirectiveToItem(
            GraphQueryExecutionContext queryContext,
            IDocumentPart targetDocumentPart,
            IDirectiveDocumentPart directiveDocumentPart,
            CancellationToken cancelToken)
        {
            var targetDirective = directiveDocumentPart.GraphType as IDirective;
            if (targetDirective == null)
            {
                // it should be impossible for this line to execute
                var directiveName = directiveDocumentPart.DirectiveName ?? "-unknown-";
                var failureMessage =
                    $"Document Directive Invocation Failure. " +
                    $"The directive type named '{directiveName}' " +
                    $"does not represent a valid directive on the target schema. (Schema: {_schema.Name})";

                throw new GraphExecutionException(
                    failureMessage,
                    targetDocumentPart.Node.Location.AsOrigin());
            }

            var inputArgs = this.GatherInputArguments(
                queryContext,
                targetDirective,
                directiveDocumentPart);

            var parentRequest = queryContext.ParentRequest;

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
                queryContext,
                request,
                queryContext.ResolvedVariables);

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

            if (!context.Messages.IsSucessful)
            {
                // if the directive execution provided meaningful failure messages
                // such as validation failures use those
                queryContext.Messages.AddRange(context.Messages);
            }
            else if (context.IsCancelled)
            {
                // when the context is just flat out canceled ensure a causal exception
                // is availabe then throw it

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
            GraphQueryExecutionContext queryContext,
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
                    queryContext.Messages.Add(argResult.Message);
            }

            return collection;
        }
    }
}