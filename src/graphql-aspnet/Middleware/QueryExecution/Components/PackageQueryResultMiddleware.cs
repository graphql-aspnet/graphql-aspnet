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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Execution.Response;
    using GraphQL.AspNet.Interfaces.Execution.Response;
    using GraphQL.AspNet.Interfaces.Middleware;

    /// <summary>
    /// Compiles the final graphql result from the individually resolved fields. This result can then be serialized.
    /// </summary>
    public class PackageQueryResultMiddleware : IQueryExecutionMiddleware
    {
        /// <inheritdoc />
        public Task InvokeAsync(QueryExecutionContext context, GraphMiddlewareInvocationDelegate<QueryExecutionContext> next, CancellationToken cancelToken)
        {
            // create and attach the result
            var fieldSet = this.CreateFinalDictionary(context);

            context.Result = new QueryExecutionResult(context.QueryRequest, context.Messages, fieldSet, context.Metrics);
            context.Logger?.RequestCompleted(context);
            return next(context, cancelToken);
        }

        /// <summary>
        /// Convert the data items that were generated, pushing their results into a final top level dictionary
        /// to be returned as the graph projection. Takes care of an final messaging in case one of the tasks failed.
        /// </summary>
        /// <param name="context">The execution context to extract reponse info from.</param>
        /// <returns>The final response field map representing the top level "data" object.</returns>
        protected virtual IQueryResponseFieldSet CreateFinalDictionary(QueryExecutionContext context)
        {
            if (context.FieldResults == null ||
                context.FieldResults.Count == 0 ||
                context.FieldResults.All(x => x.Status != FieldDataItemResolutionStatus.Complete))
            {
                // rule 6.4.4 if any top level field is nulled out or otherwise
                // made into an error state because of an error it caused or one of its child
                // fields caused then null out the "data" field entirely
                return null;
            }

            var response = new ResponseFieldSet();
            var atLeastOneFieldGenerated = false;
            foreach (var fieldResult in context.FieldResults)
            {
                var generated = fieldResult.GenerateResult(out var result);
                if (generated)
                {
                    atLeastOneFieldGenerated = true;
                    response.Add(fieldResult.Name, result);
                }
            }

            // some fields in the response set may not generate final data
            // and are allowed to be dropped. For instance when resolving a top-level field
            // that returns a union and the spread of union types in the query does not included a projection of
            // the actual data items returned. (i.e. spread on ObjectA, but only items of ObjectB were resolved
            // from the field)
            return atLeastOneFieldGenerated
                ? response
                : null;
        }
    }
}