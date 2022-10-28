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
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;

    /// <summary>
    /// Attempts to generate a valid syntax tree for the incoming query text when needed. Skipped if a query plan was pulled
    /// from the global cache.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component parses
    /// documents for.</typeparam>
    public class ParseQueryDocumentMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly IGraphQLDocumentParser _parser;
        private readonly IGraphQueryDocumentGenerator<TSchema> _documentGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseQueryDocumentMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="documentGenerator">The document generator used to convert syntax
        /// trees into functional documents.</param>
        public ParseQueryDocumentMiddleware(
            IGraphQLDocumentParser parser,
            IGraphQueryDocumentGenerator<TSchema> documentGenerator)
        {
            _parser = Validation.ThrowIfNullOrReturn(parser, nameof(parser));
            _documentGenerator = Validation.ThrowIfNullOrReturn(documentGenerator, nameof(documentGenerator));
        }

        /// <inheritdoc />
        public Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.QueryPlan == null)
            {
                context.Metrics?.StartPhase(ApolloExecutionPhase.PARSING);

                try
                {
                    // parse the text into an AST
                    var syntaxTree = _parser.ParseQueryDocument(context.OperationRequest.QueryText?.AsMemory() ?? ReadOnlyMemory<char>.Empty);
                    using (syntaxTree)
                    {
                        // convert the AST into a functional document
                        // matched against the target schema
                        var document = _documentGenerator.CreateDocument(syntaxTree);
                        context.QueryDocument = document;
                        context.Messages.AddRange(document.Messages);
                    }
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

                if (!context.Messages.IsSucessful)
                    context.Cancel();
            }

            return next(context, cancelToken);
        }
    }
}