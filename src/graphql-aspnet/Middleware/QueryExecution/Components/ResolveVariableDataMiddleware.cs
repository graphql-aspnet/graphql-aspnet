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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Resolve the variable data supplied (e.g. the json data) against the chosen operation and ensure the supplied, external variable values
    /// can be used coerced correctly for each variable usage.
    /// </summary>
    /// <typeparam name="TSchema">The type of schema this middleware component works for.</typeparam>
    internal class ResolveVariableDataMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveVariableDataMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema instance.</param>
        public ResolveVariableDataMiddleware(TSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <inheritdoc />
        public Task InvokeAsync(QueryExecutionContext context, GraphMiddlewareInvocationDelegate<QueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.IsValid && !context.IsCancelled)
            {
                // Primary Variable Checks
                // ------------------------------
                // resolves all variables and checks that they match the signatures of the core graph types
                // where they will be used
                IVariableCollectionDocumentPart variableDeclarations = null;
                if (context.QueryPlan != null)
                    variableDeclarations = context.QueryPlan.Operation.Variables;
                else if (context.Operation != null)
                    variableDeclarations = context.Operation.Variables;

                if (variableDeclarations is not null)
                {
                    // primary validation occurs during resolution
                    var variableResolver = new ResolvedVariableGenerator(_schema, variableDeclarations, context.Messages);
                    context.ResolvedVariables = variableResolver.Resolve(context.QueryRequest.VariableData);
                }

                // Secondary Variable Checks
                // --------------------------------------------
                // we now have variables resolved with data and the data matches the
                // graph types where they are used.
                // We need a hook to allow "things" (e.g. user code) to perform some data-specific checks as part of their operations.
                // (example: the @oneOf directive invokes a rule that resolved variables to input arguments must define exacty one field)
                //
                // this validation only occurs IF there is a document that we are processing against
                //
                // Note: there are several instances where the context contains an active query plan and NOT
                //       a document. We don't need to validate the variables against the plan as that indicates that
                //       variable validation already occured for the operation on the plan.  For example,
                //       when executing subscription events, the server has already resolved the query plan when it was first setup
                //       and the variable data (which can't change on sub. event messages) has already been validated.
                if (context.IsValid && !context.IsCancelled && context.QueryDocument is not null && context.Operation is not null)
                {
                    var variableDataProcessor = new VariableDataValidationRuleProcessor();
                    var validationContext = new VariableDataValidationContext(
                        _schema,
                        context.QueryDocument,
                        context.Operation,
                        context.ResolvedVariables,
                        context.Messages);

                    variableDataProcessor.Execute(validationContext);

                    if (!context.Messages.IsSucessful)
                        context.Cancel();
                }
            }

            return next(context, cancelToken);
        }
    }
}