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
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;

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
        private readonly IQueryDocumentGenerator<TSchema> _documentGenerator;

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
            IQueryDocumentGenerator<TSchema> documentGenerator)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _directivePipeline = Validation.ThrowIfNullOrReturn(directivePipeline, nameof(directivePipeline));
            _documentGenerator = Validation.ThrowIfNullOrReturn(documentGenerator, nameof(documentGenerator));
        }

        /// <inheritdoc />
        public async Task InvokeAsync(
            QueryExecutionContext context,
            GraphMiddlewareInvocationDelegate<QueryExecutionContext> next,
            CancellationToken cancelToken = default)
        {
            if (context.IsValid && context.QueryPlan == null && context.Operation != null)
            {
                var directivesToExecute = this.DetermineDirectiveToExecute(context);
                if (directivesToExecute.Count > 0)
                {
                    // when there are directives to apply, start the exectuion phase
                    // early
                    context.Metrics?.StartPhase(ApolloExecutionPhase.EXECUTION);

                    var totalDirectivesApplied = await this.ApplyDirectivesAsync(
                        context,
                        directivesToExecute,
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

        private List<IDirectiveDocumentPart> DetermineDirectiveToExecute(QueryExecutionContext context)
        {
            var list = new List<IDirectiveDocumentPart>(context.Operation.AllDirectives.Count);
            list.AddRange(context.Operation.AllDirectives);

            // fragments may be spread more than once in a single operation
            // but we dont want to execute the directives of the fragment more than once
            var includedFragments = new HashSet<string>();
            for (var i = 0; i < context.Operation.FragmentSpreads.Count; i++)
            {
                var spread = context.Operation.FragmentSpreads[i];
                if (string.IsNullOrWhiteSpace(spread.Fragment?.Name))
                    continue;

                if (includedFragments.Contains(spread.Fragment.Name))
                    continue;

                includedFragments.Add(spread.Fragment.Name);
                list.AddRange(spread.Fragment.AllDirectives);
            }

            return list;
        }

        private async Task<int> ApplyDirectivesAsync(
            QueryExecutionContext context,
            List<IDirectiveDocumentPart> directivesToExecute,
            CancellationToken cancelToken)
        {
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
                await this.ApplyDirectiveToItemAsync(
                    context,
                    targetPart,
                    directiveDocumentPart,
                    cancelToken);

                totalApplied++;
            }

            return totalApplied;
        }

        private async Task ApplyDirectiveToItemAsync(
            QueryExecutionContext queryContext,
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
                    targetDocumentPart.SourceLocation.AsOrigin());
            }

            var inputArgs = this.GatherInputArguments(
                queryContext,
                targetDirective,
                directiveDocumentPart);

            var parentRequest = queryContext.OperationRequest;

            var invocationContext = new DirectiveInvocationContext(
                targetDirective,
                directiveDocumentPart.Location,
                targetDocumentPart.SourceLocation.AsOrigin(),
                inputArgs);

            var request = new GraphDirectiveRequest(
                queryContext.OperationRequest,
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
                queryContext.Cancel();
            }
            else if (context.IsCancelled)
            {
                // when the context is just flat out canceled (user returns this.Cancel())
                // just apend a message to the query contex stating such
                queryContext.Messages.Critical(
                    $"The request was cancelled while applying the execution directive '{targetDirective.Name}' to the query document. " +
                    (causalException != null ? "See inner exception(s) for details." : string.Empty),
                    Constants.ErrorCodes.REQUEST_ABORTED,
                    targetDocumentPart.SourceLocation.AsOrigin(),
                    exceptionThrown: causalException);

                queryContext.Cancel();
            }

            if (!context.IsCancelled)
                queryContext.Logger?.ExecutionDirectiveApplied<TSchema>(context.Directive, directiveDocumentPart);
        }

        private IInputArgumentCollection GatherInputArguments(
            QueryExecutionContext queryContext,
            IDirective targetDirective,
            IDirectiveDocumentPart directivePart)
        {
            var argGenerator = new ArgumentGenerator(_schema, directivePart.Arguments);

            var collection = new InputArgumentCollection(targetDirective.Arguments.Count);
            for (var i = 0; i < targetDirective.Arguments.Count; i++)
            {
                var directiveArg = targetDirective.Arguments[i];
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