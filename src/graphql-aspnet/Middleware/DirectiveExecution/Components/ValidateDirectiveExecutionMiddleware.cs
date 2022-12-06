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
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;

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
            this.ValidateContext(context);
            return next.Invoke(context, cancelToken);
        }

        /// <summary>
        /// When overriden in a child class, allows for changing out the validation logic
        /// if necessary.
        /// </summary>
        /// <param name="context">The context to validate.</param>
        protected virtual void ValidateContext(GraphDirectiveExecutionContext context)
        {
            // execution directives are validated as part of the document generation
            // process, however; schema item directives have no such luxury.
            if (context.Request.DirectivePhase == DirectiveInvocationPhase.SchemaGeneration)
            {
                var validationProcessor = new DirectiveValidationRuleProcessor();
                validationProcessor.Execute(context);
            }
        }
    }
}