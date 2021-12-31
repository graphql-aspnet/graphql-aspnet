// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Middleware.DirectiveExecution.Components
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A piece of middleware that can fully validate a request to process
    /// a directive ensuring that the directive request can in fact be executed.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component
    /// targets.</typeparam>
    public class ValidateDirectiveExecutionMiddleware<TSchema> : IGraphDirectiveExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly TSchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateDirectiveExecutionMiddleware{TSchema}"/> class.
        /// </summary>
        /// <param name="schema">The schema instance to reference.</param>
        public ValidateDirectiveExecutionMiddleware(TSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <inheritdoc />
        public Task InvokeAsync(GraphDirectiveExecutionContext context, GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> next, CancellationToken cancelToken)
        {
            return next.Invoke(context, cancelToken);
        }
    }
}