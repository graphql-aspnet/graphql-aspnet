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
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQLSyntaxException2 = GraphQL.AspNet.Execution.Parsing.Exceptions.GraphQLSyntaxException;

    /// <summary>
    /// Attempts to generate a valid syntax tree for the incoming query text when needed. Skipped if a query plan was pulled
    /// from the global cache.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component parses
    /// documents for.</typeparam>
    public class ParseQueryDocumentMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly IQueryDocumentGenerator<TSchema> _documentGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseQueryDocumentMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="documentGenerator">The document generator used to convert syntax
        /// trees into functional documents.</param>
        public ParseQueryDocumentMiddleware(
            IQueryDocumentGenerator<TSchema> documentGenerator)
        {
            _documentGenerator = Validation.ThrowIfNullOrReturn(documentGenerator, nameof(documentGenerator));
        }

        /// <inheritdoc />
        public Task InvokeAsync(QueryExecutionContext context, GraphMiddlewareInvocationDelegate<QueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.QueryPlan == null)
            {
                context.Metrics?.StartPhase(ApolloExecutionPhase.PARSING);

                try
                {
                    var text = ReadOnlySpan<char>.Empty;
                    if (context.QueryRequest.QueryText != null)
                        text = context.QueryRequest.QueryText.AsSpan();

                    // convert the AST into a functional document
                    // matched against the target schema
                    var document = _documentGenerator.CreateDocument(text);
                    context.QueryDocument = document;
                    context.Messages.AddRange(document.Messages);
                }
                catch (GraphQLSyntaxException2 syntaxException)
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

                if (!context.Messages.IsSucessful)
                    context.Cancel();
            }

            return next(context, cancelToken);
        }
    }
}