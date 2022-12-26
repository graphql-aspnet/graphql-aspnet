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
            if (context.IsValid)
            {
                if (context.QueryPlan != null)
                {
                    this.ResolveVariables(
                        context,
                        context.QueryPlan.Operation.Variables,
                        context.OperationRequest.VariableData);
                }
                else if (context.Operation != null)
                {
                    this.ResolveVariables(
                        context,
                        context.Operation.Variables,
                        context.OperationRequest.VariableData);
                }
            }

            return next(context, cancelToken);
        }

        private void ResolveVariables(
            QueryExecutionContext context,
            IVariableCollectionDocumentPart declaredVariables,
            IInputVariableCollection suppliedVariableData)
        {
            // Convert the supplied variable values to usable objects of the type expression
            // of the chosen operation
            try
            {
                var variableResolver = new ResolvedVariableGenerator(_schema, declaredVariables);
                context.ResolvedVariables = variableResolver.Resolve(suppliedVariableData);

                // nullability checks for each resolved variable
                foreach (var resolvedVariable in context.ResolvedVariables.Values)
                {
                    if (!resolvedVariable.TypeExpression.IsNullable && resolvedVariable.Value == null)
                    {
                        context.Messages.Critical(
                             "The resolved variable value of <null> is not valid for non-nullable variable " +
                             $"'{resolvedVariable.Name}'",
                             Constants.ErrorCodes.INVALID_VARIABLE_VALUE,
                             context.Operation.SourceLocation.AsOrigin());
                    }
                }
            }
            catch (UnresolvedValueException svce)
            {
                context.Messages.Critical(
                   svce.Message,
                   Constants.ErrorCodes.INVALID_VARIABLE_VALUE,
                   context.Operation.SourceLocation.AsOrigin(),
                   exceptionThrown: svce.InnerException);
            }
        }
    }
}