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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A piece of middleware that can execute on a request to process
    /// a target object against a directive.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component
    /// targets.</typeparam>
    public class InvokeDirectiveResolverMiddleware<TSchema> : IDirectiveExecutionMiddleware
        where TSchema : class, ISchema
    {
        /// <inheritdoc />
        public async Task InvokeAsync(GraphDirectiveExecutionContext context, GraphMiddlewareInvocationDelegate<GraphDirectiveExecutionContext> next, CancellationToken cancelToken = default)
        {
            // create a collection of arguments used to invoke the
            // directive
            // build a collection of invokable parameters from the supplied context
            if (context.IsValid && !context.IsCancelled)
            {
                var isSuccessful = ExecutionArgumentGenerator.TryConvert(
                    context.Request.InvocationContext.Arguments,
                    context.VariableData,
                    context.Messages,
                    out var executionArguments);

                if (!isSuccessful)
                {
                    context.Cancel();
                }
                else
                {
                    var resolutionContext = new DirectiveResolutionContext(
                        context.Schema,
                        context,
                        context.Request,
                        executionArguments,
                        context.User);

                    // execute the directive
                    await context
                        .Directive
                        .Resolver
                        .ResolveAsync(resolutionContext, cancelToken)
                        .ConfigureAwait(false);

                    context.Messages.AddRange(resolutionContext.Messages);

                    if (resolutionContext.IsCancelled)
                        context.Cancel();
                }
            }

            await next.Invoke(context, cancelToken);
        }
    }
}