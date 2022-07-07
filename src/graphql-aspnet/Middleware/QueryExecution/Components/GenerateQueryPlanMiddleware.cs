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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Creates a new query plan from a parsed syntax tree when required.
    /// </summary>
    /// <typeparam name="TSchema">The type of schema this middleware component works for.</typeparam>
    public class GenerateQueryPlanMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly IGraphQueryDocumentGenerator<TSchema> _documentGenerator;
        private readonly IGraphQueryPlanGenerator<TSchema> _planGenerator;
        private readonly ISchemaPipeline<TSchema, GraphDirectiveExecutionContext> _directivePipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateQueryPlanMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="documentGenerator">The document generator used to validate
        /// document instances.</param>
        /// <param name="planGenerator">The plan generator.</param>
        /// <param name="directiveExecutionPipeline">The directive execution pipeline used to
        /// execute directives declared on a query document.</param>
        public GenerateQueryPlanMiddleware(
            IGraphQueryDocumentGenerator<TSchema> documentGenerator,
            IGraphQueryPlanGenerator<TSchema> planGenerator,
            ISchemaPipeline<TSchema, GraphDirectiveExecutionContext> directiveExecutionPipeline)
        {
            _documentGenerator = Validation.ThrowIfNullOrReturn(documentGenerator, nameof(documentGenerator));
            _planGenerator = Validation.ThrowIfNullOrReturn(planGenerator, nameof(planGenerator));
            _directivePipeline = Validation.ThrowIfNullOrReturn(directiveExecutionPipeline, nameof(directiveExecutionPipeline));
        }

        /// <inheritdoc />
        public async Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.IsValid && context.QueryPlan == null && context.QueryDocument != null)
            {
                context.Metrics?.StartPhase(ApolloExecutionPhase.VALIDATION);

                var document = context.QueryDocument;

                // ------------------------------------------
                // Step 1: Perform a first pass validation
                // -----------------------------------------
                // Validate the document that was just created to make sure its
                // executable against the target schema
                _documentGenerator.ValidateDocument(document);
                if (!document.Messages.IsSucessful)
                {
                    context.Messages.AddRange(document.Messages);
                    context.Cancel();
                }

                // ------------------------------------------
                // Step 2: Execute Execution Directives
                // -----------------------------------------
                // We have a document that is, as of right now, executable and resolvable
                // however, execution directives may be included and need to be processed
                // against the document parts to potentially alter them before
                // the plan is generated
                var totalExecutedDirectives = 0;
                if (context.IsValid && !context.IsCancelled)
                {
                    totalExecutedDirectives = await this.ExecuteDirectives(context, document);
                }

                // ------------------------------------------
                // Step 3: Perform a second pass validation
                // -----------------------------------------
                // Since the user is in charge of the directives we have no idea what
                // code they may have executed or what tehy may have done to the query document
                // we need to perform another full validation before we can execute against it
                if (context.IsValid && !context.IsCancelled && totalExecutedDirectives > 0)
                {
                    _documentGenerator.ValidateDocument(document);
                    if (!document.Messages.IsSucessful)
                    {
                        context.Messages.AddRange(document.Messages);
                        context.Cancel();
                    }
                }

                // ------------------------------------------
                // Step 3: Generate the final execution plan
                // -----------------------------------------
                // With the now fully complete query document, create a query plan
                // that will resolve the expected fields in the expected order to generate
                // a data result
                if (context.IsValid && !context.IsCancelled)
                {
                    context.QueryPlan = await _planGenerator.CreatePlan(context.QueryDocument).ConfigureAwait(false);
                    context.Messages.AddRange(context.QueryPlan.Messages);
                    context.Metrics?.EndPhase(ApolloExecutionPhase.VALIDATION);

                    if (context.QueryPlan.IsValid)
                        context.Logger?.QueryPlanGenerated(context.QueryPlan);
                }
            }

            await next(context, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Invokes the directive pipeline for all execution directives defined on the query
        /// document.
        /// </summary>
        /// <param name="context">The master context being executed.</param>
        /// <param name="document">The document to search for directives within.</param>
        /// <returns>Task.</returns>
        private async Task<int> ExecuteDirectives(GraphQueryExecutionContext context, IGraphQueryDocument document)
        {
            var totalDirectivesExecuted = 0;
            var allTopParts = document.Operations.Values
                .OfType<ITopLevelDocumentPart>()
                .Concat(document.NamedFragments.Values);

            foreach (ITopLevelDocumentPart topLevelPart in allTopParts)
            {
                if (topLevelPart.AllDirectives.Count == 0)
                    continue;

                foreach (var directive in topLevelPart.AllDirectives)
                {
                    // order of execution must be predictable, we can't execute
                    // all directives at once
                    await this.ExecuteDirective(context, directive.Parent, directive);
                    totalDirectivesExecuted += 1;
                }
            }

            return totalDirectivesExecuted;
        }

        /// <summary>
        /// Executes the single directive against its target document part.
        /// </summary>
        /// <param name="context">The master context being executed.</param>
        /// <param name="targetPart">The document part targeted by the directive.</param>
        /// <param name="directive">The directive being invoked.</param>
        private async Task ExecuteDirective(
            GraphQueryExecutionContext context,
            IDocumentPart targetPart,
            IDirectiveDocumentPart directive)
        {
        }
    }
}