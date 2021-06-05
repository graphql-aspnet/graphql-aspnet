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
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;

    /// <summary>
    /// Attempts to generate a valid syntax tree for the incoming query text when needed. Skipped if a query plan was pulled
    /// from the global cache.
    /// </summary>
    public class ParseQueryDocumentMiddleware : IQueryExecutionMiddleware
    {
        private readonly IGraphQLDocumentParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseQueryDocumentMiddleware"/> class.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public ParseQueryDocumentMiddleware(IGraphQLDocumentParser parser)
        {
            _parser = Validation.ThrowIfNullOrReturn(parser, nameof(parser));
        }

        /// <summary>
        /// Invokes this middleware component allowing it to perform its work against the supplied context.
        /// </summary>
        /// <param name="context">The context containing the request passed through the pipeline.</param>
        /// <param name="next">The delegate pointing to the next piece of middleware to be invoked.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        public Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.QueryPlan == null)
            {
                context.Metrics?.StartPhase(ApolloExecutionPhase.PARSING);

                try
                {
                    context.SyntaxTree = _parser.ParseQueryDocument(context.Request.QueryText?.AsMemory() ?? ReadOnlyMemory<char>.Empty);
                }
                catch (GraphQLSyntaxException syntaxException)
                {
                    // expose syntax exception messages to the client
                    // the parser is self contained and ensures its exception messages are
                    // related to the text being parsed
                    context.Messages.Critical(
                        syntaxException.Message,
                        Constants.ErrorCodes.SYNTAX_ERROR,
                        syntaxException.Location.AsOrigin());
                }
                finally
                {
                    context.Metrics?.EndPhase(ApolloExecutionPhase.PARSING);
                }
            }

            return next(context, cancelToken);
        }
    }
}