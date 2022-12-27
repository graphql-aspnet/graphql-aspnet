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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
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
            IQueryResponseFieldSet fieldSet = null;
            if (context.FieldResults != null && context.FieldResults.Any())
            {
                fieldSet = this.CreateFinalDictionary(context);
            }

            context.Result = new QueryExecutionResult(context.QueryRequest, context.Messages, fieldSet, context.Metrics);
            context.Logger?.RequestCompleted(context);
            return next(context, cancelToken);
        }

        /// <summary>
        /// Convert the data items that were generated, pushing their results into a final top level dictionary
        /// to be returned as the graph projection. Takes care of an final messaging in case one of the tasks failed.
        /// </summary>
        /// <param name="context">The execution context to extract reponse info from.</param>
        /// <returns>GraphQL.AspNet.Interfaces.Response.IResponseFieldSet.</returns>
        private IQueryResponseFieldSet CreateFinalDictionary(QueryExecutionContext context)
        {
            var topFieldResponses = new ResponseFieldSet();
            foreach (var fieldResult in context.FieldResults)
            {
                var generated = fieldResult.GenerateResult(out var result);
                if (generated)
                    topFieldResponses.Add(fieldResult.Name, result);
            }

            if (topFieldResponses.Fields.Count == 0 || topFieldResponses.Fields.All(x => x.Value == null))
                topFieldResponses = null;

            return topFieldResponses;
        }
    }
}