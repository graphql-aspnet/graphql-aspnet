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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Validates that for a chosen operation, the supplied, external variable values can be coerced into
    /// the required types for the operation.
    /// </summary>
    /// <typeparam name="TSchema">The type of schema this middleware component works for.</typeparam>
    internal class ValidateOperationVariableDataMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateOperationVariableDataMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema instance.</param>
        public ValidateOperationVariableDataMiddleware(TSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <inheritdoc />
        public Task InvokeAsync(QueryExecutionContext context, GraphMiddlewareInvocationDelegate<QueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.IsValid && !context.IsCancelled)
            {
                IVariableCollectionDocumentPart variableDeclarations = null;
                if (context.QueryPlan != null)
                    variableDeclarations = context.QueryPlan.Operation.Variables;
                else if (context.Operation != null)
                    variableDeclarations = context.Operation.Variables;

                if (variableDeclarations != null)
                {
                    var variableResolver = new ResolvedVariableGenerator(_schema, variableDeclarations, context.Messages);
                    context.ResolvedVariables = variableResolver.Resolve(context.QueryRequest.VariableData);
                }
            }

            return next(context, cancelToken);
        }
    }
}