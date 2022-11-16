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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A piece of middleware used to correctly log the directive after it was executed.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware applies to.</typeparam>
    public class LogDirectiveExecutionMiddleware<TSchema> : IDirectiveExecutionMiddleware
        where TSchema : class, ISchema
    {
        /// <inheritdoc />
        public Task InvokeAsync(GraphDirectiveExecutionContext context, GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> next, CancellationToken cancelToken = default)
        {
            if (context.IsValid && !context.IsCancelled)
            {
                if (context.Request.InvocationContext.Location.IsTypeDeclarationLocation()
                    && context.Request.DirectiveTarget is ISchemaItem sci)
                {
                    context.Logger?.TypeSystemDirectiveApplied<TSchema>(context.Directive, sci);
                }
                else if (context.Request.InvocationContext.Location.IsExecutionLocation()
                    && context.Request.DirectiveTarget is IDocumentPart dp)
                {
                    context.Logger?.ExecutionDirectiveApplied<TSchema>(context.Directive, dp);
                }
            }

            return next(context, cancelToken);
        }
    }
}