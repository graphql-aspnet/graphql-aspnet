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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.ValidationRules;

    /// <summary>
    /// A piece of middleware that can fully validate a request to process
    /// a directive ensuring that the directive request can in fact be executed.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component
    /// targets.</typeparam>
    public class ValidateDirectiveExecutionMiddleware<TSchema> : IDirectiveExecutionMiddleware
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateDirectiveExecutionMiddleware{TSchema}"/> class.
        /// </summary>
        public ValidateDirectiveExecutionMiddleware()
        {
        }

        /// <inheritdoc />
        public Task InvokeAsync(GraphDirectiveExecutionContext context, GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> next, CancellationToken cancelToken = default)
        {
            var validationProcessor = new DirectiveValidationRuleProcessor();
            validationProcessor.Execute(context);

            return next.Invoke(context, cancelToken);
        }
    }
}