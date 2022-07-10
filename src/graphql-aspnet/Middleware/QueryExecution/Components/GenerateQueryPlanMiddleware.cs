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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
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
        private readonly TSchema _schema;
        private readonly IGraphQueryDocumentGenerator<TSchema> _documentGenerator;
        private readonly IGraphQueryPlanGenerator<TSchema> _planGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateQueryPlanMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The target schema.</param>
        /// <param name="documentGenerator">The document generator used to validate
        /// document instances.</param>
        /// <param name="planGenerator">The plan generator.</param>
        public GenerateQueryPlanMiddleware(
            TSchema schema,
            IGraphQueryDocumentGenerator<TSchema> documentGenerator,
            IGraphQueryPlanGenerator<TSchema> planGenerator)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _documentGenerator = Validation.ThrowIfNullOrReturn(documentGenerator, nameof(documentGenerator));
            _planGenerator = Validation.ThrowIfNullOrReturn(planGenerator, nameof(planGenerator));
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
                // Step 1a: Fetch a reference to the operation to be used for the rest of
                //          the query plan generation process
                // -----------------------------------------
                // Validate the document that was just created to make sure its
                // executable against the target schema
                IOperationDocumentPart targetOperation = null;
                if (context.IsValid && !context.IsCancelled)
                {
                    if (document.Operations.Count == 1)
                        targetOperation = document.Operations[0];
                    else
                        targetOperation = document.Operations.RetrieveOperation(context.OperationRequest.OperationName);

                    if (targetOperation == null)
                    {
                        var name = context.OperationRequest.OperationName?.Trim() ?? "~anonymous~";
                        context.Messages.Critical(
                            $"Undeclared operation. An operation with the name '{name}' was not " +
                            "found on the query document.",
                            Constants.ErrorCodes.BAD_REQUEST,
                            document.Node.Location.AsOrigin());

                        context.Cancel();
                    }
                }

                // ------------------------------------------
                // Step 2: Execute Execution Directives
                // -----------------------------------------
                // We have a document that is, as of right now, executable and resolvable,
                // however; execution directives may be included and need to be processed
                // against the document parts to potentially alter them before
                // the plan is generated
                var totalExecutedDirectives = 0;
                if (context.IsValid && !context.IsCancelled)
                {
                    var directiveProcessor = new DirectiveProcessorQueryDocument<TSchema>(_schema, context);

                    try
                    {
                        totalExecutedDirectives = await directiveProcessor
                            .ApplyDirectives(targetOperation, cancelToken)
                            .ConfigureAwait(false);
                    }
                    catch (GraphExecutionException gee)
                    {
                        context.Messages.Critical(
                            gee.Message,
                            Constants.ErrorCodes.EXECUTION_ERROR,
                            gee.Origin,
                            gee);

                        context.Cancel();
                    }
                }

                // ------------------------------------------
                // Step 3: Perform a second pass validation
                // -----------------------------------------
                // Since the user is in charge of the directives we have no idea what
                // code they may have executed or what they may have done to the query document
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
                // Step 4: Generate the final execution plan
                // -----------------------------------------
                // With the now fully complete query document, create a query plan
                // that will resolve the expected fields in the expected order to generate
                // a data result
                if (context.IsValid && !context.IsCancelled)
                {
                    context.QueryPlan = await _planGenerator
                        .CreatePlan(targetOperation)
                        .ConfigureAwait(false);

                    context.QueryPlan.IsCacheable = totalExecutedDirectives == 0;
                    context.Messages.AddRange(context.QueryPlan.Messages);
                    context.Metrics?.EndPhase(ApolloExecutionPhase.VALIDATION);

                    if (context.QueryPlan.IsValid)
                        context.Logger?.QueryPlanGenerated(context.QueryPlan);
                }
            }

            await next(context, cancelToken).ConfigureAwait(false);
        }
    }
}